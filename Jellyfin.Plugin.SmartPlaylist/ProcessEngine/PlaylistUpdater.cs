using System.Collections.Immutable;
using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;
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
	private readonly User                _user;
	private readonly BaseItemKind[]      _supportedItems;
	private readonly IFileSystem         _fileSystem;
	private readonly ILibraryManager     _libraryManager;
	private readonly ILogger             _logger;
	private readonly CancellationToken   _cancellationToken;
	private readonly IPlaylistManager    _playlistManager;
	private readonly IProviderManager    _providerManager;
	private readonly IProgress<double>   _progress;

	public PlaylistUpdater(User                user,
						   BaseItemKind[]      supportedItems,
						   IFileSystem         fileSystem,
						   ILibraryManager     libraryManager,
						   IPlaylistManager    playlistManager,
						   IProviderManager    providerManager,
						   ILogger             logger,
						   CancellationToken   cancellationToken,
						   IProgress<double>   progress) {
		_user                  = user;
		_fileSystem            = fileSystem;
		_supportedItems        = supportedItems;
		_libraryManager        = libraryManager;
		_logger                = logger;
		_cancellationToken     = cancellationToken;
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

	public async Task ProcessPlaylists(IEnumerable<PlaylistIoData> playlists) {
		var listsToProcess =   GetBuiltPlaylists(playlists);
		var items          = GetAllUserMedia();
		var progress       = new ProgressTracker(_progress) {
			Length = items.Count,
		};

		var numComplete = 0;

        for (var index = 0; index < items.Count; index++) {
			_cancellationToken.ThrowIfCancellationRequested();

			var opp = new Operand(_libraryManager, items[index], BaseItem.UserDataManager, _user);

			foreach (var list in listsToProcess) {
				try {
					list.Sorter.SortItem(opp);
				}
				catch (Exception e) {
					list.IoData.UpdatePlaylistRun($"Error sorting data: {e.Message}");
				}
			}

			progress.Index = index;
			progress.Report(numComplete++ / (double)items.Count);
		}

		foreach (var list in listsToProcess) {
			_cancellationToken.ThrowIfCancellationRequested();

			try {
				Guid[] newItems;

				if (list.SmartPlaylist.MaxItems > 0) {
					newItems = list.Sorter.GetResults().Take(list.SmartPlaylist.MaxItems).ToArray();
				}
				else {
					newItems = list.Sorter.GetResults().ToArray();
				}

				if (list.Playlist is null) {
					CreateNewPlaylist(list.SmartPlaylist.Dto, newItems);
					SmartPlaylistManager.SavePlaylist(list.SmartPlaylist.Dto.FileName, list.SmartPlaylist.Dto);
				}
				else {
					ClearPlaylist(list.Playlist, newItems);

					await _playlistManager.AddToPlaylistAsync(list.Playlist.Id, newItems, _user.Id)
										  .ConfigureAwait(false);
				}
			}
			catch (Exception e) {
				list.IoData.UpdatePlaylistRun($"Error sorting data: {e.Message}");
			}
		}
	}

	private void ClearPlaylist(Playlist playlist, Guid[] newItems) {
		var children = playlist.GetManageableItems();

		playlist.LinkedChildren = children.Where(a => a.Item1.ItemId is not null && !newItems.Contains(a.Item1.ItemId.Value))
										  .Select(i => i.Item1)
										  .ToArray();

		playlist.UpdateToRepositoryAsync(ItemUpdateType.MetadataEdit, CancellationToken.None);

		_providerManager.QueueRefresh(playlist.Id,
									  new(new DirectoryService(_fileSystem)) {
											  ForceSave = true,
									  },
									  RefreshPriority.High);
	}

	private List<PlaylistPair> GetBuiltPlaylists(IEnumerable<PlaylistIoData> playlists) {
		var userLists = GetUserPlaylists().ToArray();
		var pairs     = new List<PlaylistPair>(20);

		foreach (var dto in playlists) {
			if (dto.SmartPlaylist is null) {
				SmartPlaylistManager.UpdatePlaylistRun(dto.FileId, "Playlist is null");
				continue;
			}
			var      pl = new Models.SmartPlaylist(dto.SmartPlaylist);
			Playlist jfList;

			try {
				pl.CompileRules();
			}
			catch (Exception ex) {
				SmartPlaylistManager.UpdatePlaylistRun(dto.FileId, $"Playlist failed to compile: {ex.Message}");
				_logger.LogError(ex, "Error parsing rules for {FileName}", pl.FileName);

				continue;
			}

			try {
				jfList = userLists.FirstOrDefault(x => x.Id.ToString("N") == dto.SmartPlaylist.Id);
			}
			catch (Exception ex) {
				SmartPlaylistManager.UpdatePlaylistRun(dto.FileId, $"Error finding the playlist: {ex.Message}");
				_logger.LogError(ex, "Error finding Jellyfin Playlist {PlaylistName}", dto.SmartPlaylist.Name);

				continue;
			}


			pairs.Add(new(pl, jfList, dto, pl.GetSorter()));
		}

		return pairs;
	}

	private IEnumerable<Playlist> GetUserPlaylists() => _playlistManager.GetPlaylists(_user.Id);

	private void CreateNewPlaylist(SmartPlaylistDto dto, IReadOnlyList<Guid> items) {
		var req = new PlaylistCreationRequest {
				Name       = dto.Name,
				UserId     = _user.Id,
				ItemIdList = items,
		};

		var foo = _playlistManager.CreatePlaylist(req);
		dto.Id = foo.Result.Id;
	}

	private record PlaylistPair(Models.SmartPlaylist        SmartPlaylist,
								Playlist?                   Playlist,
								PlaylistIoData              IoData,
								Models.SmartPlaylist.Sorter Sorter)
	{
		public bool HasErrorOccurred { get; set; }
	}
}
