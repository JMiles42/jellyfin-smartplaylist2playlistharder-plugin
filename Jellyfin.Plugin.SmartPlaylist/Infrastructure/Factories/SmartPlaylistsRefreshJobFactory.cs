using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;

public sealed class SmartPlaylistsRefreshJobFactory
{
    private readonly ILoggerFactory _logger;
    private readonly ISmartPlaylistManager _smartPlaylistManager;
    private readonly IPlaylistManager _playlistManager;
    private readonly IProviderManager _providerManager;
    private readonly IDirectoryService _directoryService;
    private readonly IUserManager _userManager;

    private readonly ISmartPlaylistPluginConfiguration _config;

    public SmartPlaylistsRefreshJobFactory(ILoggerFactory logger,
                                           IPlaylistManager playlistManager,
                                           ISmartPlaylistManager smartPlaylistManager,
                                           IProviderManager providerManager,
                                           IDirectoryService directoryService,
                                           ISmartPlaylistPluginConfiguration config,
                                           IUserManager userManager)
    {
        _logger = logger;
        _smartPlaylistManager = smartPlaylistManager;
        _playlistManager = playlistManager;
        _providerManager = providerManager;
        _directoryService = directoryService;
        _config = config;
        _userManager = userManager;
    }

    public SmartPlaylistsRefreshJob BuildJob(PlaylistProcessRunData playlistProcessRunData) =>
            new(playlistProcessRunData,
                 _logger.CreateLogger<SmartPlaylistsRefreshJob>(),
                 _playlistManager,
                 _smartPlaylistManager,
                 _providerManager,
                 _directoryService,
                 _config,
                 _userManager);
}