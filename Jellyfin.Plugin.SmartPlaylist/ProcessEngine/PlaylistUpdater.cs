using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Playlists;

namespace Jellyfin.Plugin.SmartPlaylist.ProcessEngine;

public class PlaylistUpdater
{
	private readonly User              _user;
	private readonly BaseItemKind[]    _supportedItems;
	private readonly IFileSystem       _fileSystem;
	private readonly ILibraryManager   _libraryManager;
	private readonly ILogger           _logger;
	private readonly IPlaylistManager  _playlistManager;
	private readonly IProviderManager  _providerManager;
	private readonly IProgress<double> _progress;

	public PlaylistUpdater(User                user,
						   BaseItemKind[]      supportedItems,
						   IFileSystem         fileSystem,
						   ILibraryManager     libraryManager,
						   IPlaylistManager    playlistManager,
						   IProviderManager    providerManager,
						   ILogger             logger,
						   IProgress<double>   progress) {
		_user                  = user;
		_fileSystem            = fileSystem;
		_supportedItems        = supportedItems;
		_libraryManager        = libraryManager;
		_logger                = logger;
		_playlistManager       = playlistManager;
		_providerManager       = providerManager;
		_progress              = progress;
	}

	private IReadOnlyList<BaseItem> GetAllUserMedia() {
		var query = new InternalItemsQuery(_user) {
				IncludeItemTypes = _supportedItems,
				Recursive        = true,
		};

		return _libraryManager.GetItemsResult(query).Items;
	}

	public async Task ProcessPlaylists(IEnumerable<SmartPlaylistsRefreshJob> jobsIn, CancellationToken cancellationToken) {
		var jobs = jobsIn.ToArray();
		var userPlaylists = GetUserPlaylists().ToArray();
		foreach (var job in jobs) {
			job.BuildPlaylist(userPlaylists);
		}

		var items = GetAllUserMedia();
		var progress = new ProgressTracker(_progress) {
			Length = items.Count,
		};

		var numComplete = 0;

		for (var index = 0; index < items.Count; index++) {
			cancellationToken.ThrowIfCancellationRequested();

			var opp = new Operand(_libraryManager, items[index], BaseItem.UserDataManager, _user);

			foreach (var job in jobs) {
				job.ProcessItem(opp);
			}

			progress.Index = index;
			progress.Report(numComplete++ / (double)items.Count);
		}

		foreach (var job in jobs) {
			var newItems = job.GetItems();
			await job.CreateOrUpdatePlaylist(_playlistManager,
									   _providerManager,
									   _fileSystem,
									   newItems,
									   cancellationToken);
		}

		foreach (var job in jobs) {
			if (job.HasErrors) {
				SmartPlaylistManager.SetErrorStatus(job.FileId, jobProcessErrors: job.ProcessErrors);
			}
			else {
				SmartPlaylistManager.SetStatus(job.FileId, SmartPlaylistLastRunDetails.SUCCESS, job.PlaylistId);
			}
		}
	}

	private IEnumerable<Playlist> GetUserPlaylists() => _playlistManager.GetPlaylists(_user.Id);
}
