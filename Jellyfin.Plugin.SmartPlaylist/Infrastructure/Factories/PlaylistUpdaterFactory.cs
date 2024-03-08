using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using MediaBrowser.Controller.Playlists;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;

public class PlaylistUpdaterFactory
{
	private readonly ILibraryManager                   _libraryManager;
	private readonly IPlaylistManager                  _playlistManager;
	private readonly ISmartPlaylistPluginConfiguration _config;
	private readonly IServiceProvider                  _serviceProvider;

	public PlaylistUpdaterFactory(ILibraryManager                   libraryManager,
								  ISmartPlaylistPluginConfiguration config,
								  IPlaylistManager                  playlistManager,
								  IServiceProvider serviceProvider)
	{
		_playlistManager = playlistManager;
		_libraryManager  = libraryManager;
		_config          = config;
		_serviceProvider = serviceProvider;
	}

	public PlaylistUpdater BuildUpdater(User                  users,
										BaseItemKind[]        supportedItems,
										NestedProgressTracker progress) =>
			new(users,
				supportedItems,
				progress,
				_libraryManager,
				_playlistManager,
				_config,
				_serviceProvider);
}
