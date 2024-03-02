using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using Jellyfin.Plugin.SmartPlaylist.ProcessEngine;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Playlists;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.SmartPlaylist.ScheduleTasks;

public class RefreshAllSmartPlaylists : IScheduledTask, IConfigurableScheduledTask {
    private readonly IFileSystem             _fileSystem;
    private readonly ILibraryManager         _libraryManager;
    private readonly ILogger                 _logger;
    private readonly IPlaylistManager        _playlistManager;
    private readonly IProviderManager        _providerManager;
    private readonly IUserManager            _userManager;
    private readonly IServerApplicationPaths _serverApplicationPaths;

    public RefreshAllSmartPlaylists(IFileSystem             fileSystem,
                                    ILibraryManager         libraryManager,
                                    IPlaylistManager        playlistManager,
                                    IProviderManager        providerManager,
                                    IServerApplicationPaths serverApplicationPaths,
                                    IUserManager            userManager,
                                    ILoggerFactory          loggerFactory) {
        _fileSystem             = fileSystem;
        _libraryManager         = libraryManager;
        _logger                 = loggerFactory.CreateLogger<RefreshAllSmartPlaylists>();
        _playlistManager        = playlistManager;
        _providerManager        = providerManager;
        _userManager            = userManager;
        _serverApplicationPaths = serverApplicationPaths;
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

    public string Key => nameof(RefreshAllSmartPlaylists);

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
            var folder = Path.Combine(_serverApplicationPaths.DataPath, "smartplaylists");
            var dtos   = SmartPlaylistManager.GetAllValidPlaylists(folder).ToArray();

            var groups = dtos.GroupBy(a => new {
                    a.SmartPlaylist!.User,
                    a.SmartPlaylist!.SupportedItems,
            }).ToArray();

            var tracker = new ProgressTracker(progress) {
                    Length = groups.Length,
            };

            foreach (var group in groups) {
                cancellationToken.ThrowIfCancellationRequested();
                var user = _userManager.GetUserByName(group.Key.User);

                if (user is null) {

                    foreach (var playlistIoData in group) {
                        playlistIoData.UpdatePlaylistRun($"Error User {group.Key.User} does not exist, cannot process playlist.");
                    }

                    continue;
                }

                var sorter = new PlaylistUpdater(user,
                                                 group.Key.SupportedItems,
                                                 _fileSystem,
                                                 _libraryManager,
                                                 _playlistManager,
                                                 _providerManager,
                                                 _logger,
                                                 tracker);

                await sorter.ProcessPlaylists(group, cancellationToken).ConfigureAwait(false);
                tracker.Index++;
            }
        }
        catch (OperationCanceledException) {
            return;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error processing playlists");
        }
    }
}
