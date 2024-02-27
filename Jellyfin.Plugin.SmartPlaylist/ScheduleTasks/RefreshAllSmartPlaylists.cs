using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;
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
using System;

namespace Jellyfin.Plugin.SmartPlaylist.ScheduleTasks;

public class RefreshAllSmartPlaylists : IScheduledTask, IConfigurableScheduledTask {
    private readonly IFileSystem                    _fileSystem;
    private readonly ILibraryManager                _libraryManager;
    private readonly ILogger                        _logger;
    private readonly IPlaylistManager               _playlistManager;
    private readonly ISmartPlaylistStore            _plStore;
    private readonly IProviderManager               _providerManager;
    private readonly IUserManager                   _userManager;

    public RefreshAllSmartPlaylists(IFileSystem fileSystem,
                                 ILibraryManager libraryManager,
                                 IPlaylistManager playlistManager,
                                 IProviderManager providerManager,
                                 IServerApplicationPaths serverApplicationPaths,
                                 IUserManager userManager,
                                 ILoggerFactory loggerFactory) {
        _fileSystem      = fileSystem;
        _libraryManager  = libraryManager;
        _logger          = loggerFactory.CreateLogger<RefreshAllSmartPlaylists>();
        _playlistManager = playlistManager;
        _providerManager = providerManager;
        _userManager     = userManager;

        ISmartPlaylistFileSystem plFileSystem = new SmartPlaylistFileSystem(serverApplicationPaths,
                                                                            loggerFactory.CreateLogger<SmartPlaylistFileSystem>());
        _plStore = new SmartPlaylistStore(plFileSystem, _logger);
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
            var dtos = await _plStore.GetAllSmartPlaylistAsync();

            var groups = dtos.GroupBy(a => new { a.User, a.SupportedItems });

            var tracker = new ProgressTracker(progress) {
                    Length = 3,
            };

            foreach (var group in groups) {
                cancellationToken.ThrowIfCancellationRequested();
                var user = _userManager.GetUserByName(group.Key.User);

                var sorter = new PlaylistUpdater(user,
                                                 group.Key.SupportedItems,
                                                 _fileSystem,
                                                 _libraryManager,
                                                 _playlistManager,
                                                 _providerManager,
                                                 _plStore,
                                                 _logger,
                                                 tracker);

                await sorter.ProcessPlaylists(group).ConfigureAwait(false);
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
