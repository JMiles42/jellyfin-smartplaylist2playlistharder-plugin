using MediaBrowser.Controller;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class SmartPlaylistsRefreshAll
{
	private readonly IFileSystem             _fileSystem;
	private readonly ILibraryManager         _libraryManager;
	private readonly ILogger                 _logger;
	private readonly IPlaylistManager        _playlistManager;
	private readonly IProviderManager        _providerManager;
	private readonly IUserManager            _userManager;
	private readonly IServerApplicationPaths _serverApplicationPaths;
	private readonly ILoggerFactory          _loggerFactory;

	public SmartPlaylistsRefreshAll(IFileSystem                       fileSystem,
									ILibraryManager                   libraryManager,
									IPlaylistManager                  playlistManager,
									IProviderManager                  providerManager,
									IServerApplicationPaths           serverApplicationPaths,
									IUserManager                      userManager,
									ILoggerFactory                    loggerFactory,
									ILogger<SmartPlaylistsRefreshAll> logger)
	{
		_fileSystem             = fileSystem;
		_libraryManager         = libraryManager;
		_logger                 = logger;
		_playlistManager        = playlistManager;
		_providerManager        = providerManager;
		_userManager            = userManager;
		_loggerFactory          = loggerFactory;
		_serverApplicationPaths = serverApplicationPaths;
	}

	public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
	{
		var folder = Path.Combine(_serverApplicationPaths.DataPath, "smartplaylists");
		var dtos   = SmartPlaylistManager.GetAllPlaylists(folder).ToArray();

		var jobs = dtos
				   .Select(d => new SmartPlaylistsRefreshJob(d,
															 _loggerFactory.CreateLogger<SmartPlaylistsRefreshJob>()))
				   .ToArray();

		foreach (var job in jobs)
		{
			job.SetUser(_userManager);
		}

		var jobGroups = jobs.GroupBy(a => a.GetGrouping()).ToArray();

		var tracker = new ProgressTracker(progress, jobGroups.Length);

		foreach (var group in jobGroups)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (group.Key.User is null || group.Key.Kinds is null)
			{
				foreach (var job in group)
				{
					SmartPlaylistManager.SetErrorStatus(job.FileId, jobProcessErrors: job.ProcessErrors);
				}

				continue;
			}

			var sorter = new PlaylistUpdater(group.Key.User,
											 group.Key.Kinds,
											 _fileSystem,
											 _libraryManager,
											 _playlistManager,
											 _providerManager,
											 _logger,
											 tracker);

			await sorter.ProcessPlaylists(group, cancellationToken).ConfigureAwait(false);
			tracker.Increment();
		}
	}
}
