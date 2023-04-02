using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Playlists;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.SmartPlaylist.ScheduleTasks;

public class RefreshAllPlaylists : IScheduledTask, IConfigurableScheduledTask
{
    private readonly IFileSystem _fileSystem;
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger _logger;
    private readonly IPlaylistManager _playlistManager;
    private readonly ISmartPlaylistStore _plStore;
    private readonly IProviderManager _providerManager;
    private readonly IUserManager _userManager;

    public RefreshAllPlaylists(IFileSystem fileSystem,
                               ILibraryManager libraryManager,
                               ILogger<RefreshAllPlaylists> logger,
                               IPlaylistManager playlistManager,
                               IProviderManager providerManager,
                               IServerApplicationPaths serverApplicationPaths,
                               IUserManager userManager)
    {
        _fileSystem = fileSystem;
        _libraryManager = libraryManager;
        _logger = logger;
        _playlistManager = playlistManager;
        _providerManager = providerManager;
        _userManager = userManager;

        ISmartPlaylistFileSystem plFileSystem = new SmartPlaylistFileSystem(serverApplicationPaths);
        _plStore = new SmartPlaylistStore(plFileSystem, logger);

        _logger.LogInformation("Constructed Refresher ");
    }

    private string CreateNewPlaylist(SmartPlaylistDto dto, User user)
    {
        var req = new PlaylistCreationRequest
        {
            Name = dto.Name,
            UserId = user.Id
        };

        var foo = _playlistManager.CreatePlaylist(req);

        return foo.Result.Id;
    }

    private IEnumerable<BaseItem> GetAllUserMedia(User user, BaseItemKind[] itemTypes)
    {
        var query = new InternalItemsQuery(user)
        {
            IncludeItemTypes = itemTypes,
            Recursive = true,
        };

        return _libraryManager.GetItemsResult(query).Items;
    }

    // Real PlaylistManagers RemoveFromPlaylist needs an entry ID which seems to not work. Explore further and file a bug.
    public void RemoveFromPlaylist(string playlistId, IEnumerable<string> entryIds)
    {
        if (_libraryManager.GetItemById(playlistId) is not Playlist playlist)
        {
            throw new ArgumentException("No Playlist exists with the supplied Id");
        }

        var children = playlist.GetManageableItems().ToList();

        var idList = entryIds.ToList();
        var removals = children.Where(i => idList.Contains(i.Item1.ItemId.ToString())).ToArray();

        playlist.LinkedChildren = children.Except(removals)
                                          .Select(i => i.Item1)
                                          .ToArray();

        playlist.UpdateToRepositoryAsync(ItemUpdateType.MetadataEdit, CancellationToken.None);

        _providerManager.QueueRefresh(playlist.Id,
                                      new(new DirectoryService(_fileSystem))
                                      {
                                          ForceSave = true
                                      },
                                      RefreshPriority.High);
    }

    public bool IsHidden => false;

    public bool IsEnabled => true;

    public bool IsLogged => true;

    public string Key => nameof(RefreshAllPlaylists);

    public string Name => "Refresh all SmartPlaylist";

    public string Description => "Refresh all SmartPlaylists Playlists";

    public string Category => "Smart Playlist 2 Playlist Harder";

    // TODO check for creation of schedule json file. Isn't created currently and won't execute until it is.
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[] {
                new TaskTriggerInfo {
                        IntervalTicks = TimeSpan.FromMinutes(30).Ticks,
                        Type          = TaskTriggerInfo.TriggerInterval
                }
        };
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            var dtos = await _plStore.GetAllSmartPlaylistAsync();

            foreach (var dto in dtos)
            {
                var smartPlaylist = new SmartPlaylist.Models.SmartPlaylist(dto);

                var user = _userManager.GetUserByName(smartPlaylist.User);
                progress.Report(5);
                List<Playlist> p = new();

                try
                {
                    var playlists = _playlistManager.GetPlaylists(user.Id);
                    p.AddRange(playlists.Where(x => x.Id.ToString("N") == dto.Id));
                }
                catch (NullReferenceException ex)
                {
                    _logger.LogError(ex,
                                     "No user named {User} found, please fix playlist {PlaylistName}",
                                     dto.User,
                                     dto.Name);

                    continue;
                }

                progress.Report(20);

                if ((dto.Id == null) || !p.Any())
                {
                    _logger.LogInformation("Playlist ID not set, creating new playlist");
                    var plId = CreateNewPlaylist(dto, user);
                    dto.Id = plId;
                    await _plStore.SaveAsync(dto);
                    var playlists = _playlistManager.GetPlaylists(user.Id);
                    p.Clear();
                    p.AddRange(playlists.Where(x => x.Id.ToString("N") == dto.Id));
                }

                progress.Report(30);

                var newItems = smartPlaylist.FilterPlaylistItems(GetAllUserMedia(user, smartPlaylist.SupportedItems),
                                                                 _libraryManager,
                                                                 user,
                                                                 _logger);

                progress.Report(40);
                var playlist = p.First();

                var query = new InternalItemsQuery(user)
                {
                    IncludeItemTypes = smartPlaylist.SupportedItems.ToArray(),
                    Recursive = true,
                };

                var plItems = playlist.GetChildren(user, false, query).ToList();
                progress.Report(60);

                var toRemove = plItems.Select(x => x.Id.ToString()).ToList();
                progress.Report(80);
                RemoveFromPlaylist(playlist.Id.ToString(), toRemove);
                progress.Report(95);
                await _playlistManager.AddToPlaylistAsync(playlist.Id, newItems.ToArray(), user.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing playlists");
        }
    }
}
