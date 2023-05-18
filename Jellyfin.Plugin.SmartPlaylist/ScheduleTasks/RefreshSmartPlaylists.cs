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

public class RefreshSmartPlaylists : IScheduledTask, IConfigurableScheduledTask {
    private readonly IFileSystem                    _fileSystem;
    private readonly ILibraryManager                _libraryManager;
    private readonly ILogger<RefreshSmartPlaylists> _logger;
    private readonly IPlaylistManager               _playlistManager;
    private readonly ISmartPlaylistStore            _plStore;
    private readonly IProviderManager               _providerManager;
    private readonly IUserManager                   _userManager;

    public RefreshSmartPlaylists(IFileSystem fileSystem,
                                 ILibraryManager libraryManager,
                                 ILogger<RefreshSmartPlaylists> logger,
                                 IPlaylistManager playlistManager,
                                 IProviderManager providerManager,
                                 IServerApplicationPaths serverApplicationPaths,
                                 IUserManager userManager,
                                 ILoggerFactory loggerFactory) {
        _fileSystem        = fileSystem;
        _libraryManager    = libraryManager;
        _logger            = logger;
        _playlistManager   = playlistManager;
        _providerManager   = providerManager;
        _userManager       = userManager;

        ISmartPlaylistFileSystem plFileSystem = new SmartPlaylistFileSystem(serverApplicationPaths, loggerFactory.CreateLogger<SmartPlaylistFileSystem>());
        _plStore = new SmartPlaylistStore(plFileSystem, logger);
    }

    private string CreateNewPlaylist(SmartPlaylistDto dto, User user, IReadOnlyList<Guid> items) {
        var req = new PlaylistCreationRequest {
            Name = dto.Name,
            UserId = user.Id,
            ItemIdList = items,
        };

        var foo = _playlistManager.CreatePlaylist(req);
        return foo.Result.Id;
    }

    private IEnumerable<BaseItem> GetAllUserMedia(User user, BaseItemKind[] itemTypes) {
        var query = new InternalItemsQuery(user) {
            IncludeItemTypes = itemTypes,
            Recursive = true,
        };

        return _libraryManager.GetItemsResult(query).Items;
    }

    public bool IsHidden => false;

    public bool IsEnabled => true;

    public bool IsLogged => true;

    public string Key => nameof(RefreshSmartPlaylists);

    public string Name => "Refresh all SmartPlaylist";

    public string Description => "Refresh all SmartPlaylists Playlists";

    public string Category => "Smart Playlist 2 Playlist Harder";

    // TODO check for creation of schedule json file. Isn't created currently and won't execute until it is.
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() {
        return new[] {
                new TaskTriggerInfo {
                        IntervalTicks = TimeSpan.FromHours(6).Ticks,
                        Type          = TaskTriggerInfo.TriggerInterval,
                },
        };
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken) {
        try {
            var dtos = await _plStore.GetAllSmartPlaylistAsync();

            for (var index = 0; index < dtos.Length; index++) {
                await ProcessPlaylist(dtos[index], percent => progress.ReportPercentage(dtos.Length, index, percent));
            }
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error processing playlists");
        }
    }

    private async Task ProcessPlaylist(SmartPlaylistDto dto, Action<double> progressCallback) {
        progressCallback(0);

        if (dto.IsReadonly) {
            progressCallback(100);

            return;
        }

        var smartPlaylist = new Models.SmartPlaylist(dto);

        try {
            smartPlaylist.CompileRules();
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error parsing rules for {FileName}", smartPlaylist.FileName);

            return;
        }

        var user = _userManager.GetUserByName(smartPlaylist.User);
        progressCallback(10);
        List<Playlist> p = new();

        try {
            var playlists = _playlistManager.GetPlaylists(user.Id);

            p.AddRange(playlists.Where(x => x.Id.ToString("N") == dto.Id));

            progressCallback(20);
        }
        catch (NullReferenceException ex) {
            _logger.LogError(ex,
                             "No user named {User} found, please fix playlist {PlaylistName}",
                             dto.User,
                             dto.Name);

            return;
        }

        if ((dto.Id == null) || !p.Any()) {
            await CreateNewPlaylist(dto, progressCallback, smartPlaylist, user);

            return;
        }

        await UpdateExistingPlaylist(dto, progressCallback, p.First().Id, smartPlaylist, user);
    }

    private async Task UpdateExistingPlaylist(SmartPlaylistDto dto,
                                              Action<double> progressCallback,
                                              Guid playlistId,
                                              Models.SmartPlaylist smartPlaylist,
                                              User user) {
        progressCallback(40);

        if (_libraryManager.GetItemById(playlistId) is not Playlist playlist) {
            throw new ArgumentException("No Playlist exists with the supplied Id");
        }

        //Clear playlist, this ensures only valid items, and that they are in the correct order when added.
        await _playlistManager.RemoveFromPlaylistAsync(playlist.Id.ToString(), playlist.LinkedChildren.Select(b => b.Id));

        progressCallback(50);

        playlist.Tagline = $"{dto.Name} Generated by {SmartPlaylistConsts.PLUGIN_NAME}";

        var newItems = GetPlaylistItems(smartPlaylist, user);
        progressCallback(75);

        await _playlistManager.AddToPlaylistAsync(playlist.Id, newItems, user.Id);

        progressCallback(95);
        QueueRefresh(playlist.Id);
        progressCallback(99);
    }

    private async Task CreateNewPlaylist(SmartPlaylistDto dto,
                                         Action<double> progressCallback,
                                         Models.SmartPlaylist smartPlaylist,
                                         User user) {
        _logger.LogInformation("Playlist ID not set, creating new playlist");
        progressCallback(40);
        var itemsToInitWith = GetPlaylistItems(smartPlaylist, user);
        progressCallback(60);
        var plId = CreateNewPlaylist(dto, user, itemsToInitWith);
        progressCallback(80);
        dto.Id = plId;
        await _plStore.SaveAsync(dto);
        progressCallback(99);
    }

    private Guid[] GetPlaylistItems(Models.SmartPlaylist smartPlaylist, User user) {

        var rst = smartPlaylist.FilterPlaylistItems(GetAllUserMedia(user, smartPlaylist.SupportedItems),
                                                    _libraryManager,
                                                    user);

        if (smartPlaylist.MaxItems > 0) {
            return rst.Take(smartPlaylist.MaxItems).ToArray();
        }

        return rst.ToArray();
    }

    public void QueueRefresh(Guid playlistId) {
        _providerManager.QueueRefresh(playlistId,
                                      new(new DirectoryService(_fileSystem)) {
                                          ForceSave = true,
                                          ReplaceAllImages = true,
                                      },
                                      RefreshPriority.High);
    }
}
