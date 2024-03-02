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

	public async Task ProcessPlaylists(IEnumerable<PlaylistProcessRunData> playlists, CancellationToken cancellationToken) {
		var listsToProcess =     GetBuiltPlaylists(playlists);
		var items          = GetAllUserMedia();
		var progress       = new ProgressTracker(_progress) {
			Length = items.Count,
		};

		var numComplete = 0;

        for (var index = 0; index < items.Count; index++) {
			cancellationToken.ThrowIfCancellationRequested();

			var opp = new Operand(_libraryManager, items[index], BaseItem.UserDataManager, _user);

			foreach (var playlistPair in listsToProcess) {
				try {
					playlistPair.Sorter.SortItem(opp);
				}
				catch (Exception e) {
					playlistPair.ProcessRunData.UpdatePlaylistRun("Error sorting items", e);
					playlistPair.HasErrorOccurred = true;
				}
			}

			progress.Index = index;
			progress.Report(numComplete++ / (double)items.Count);
		}

		foreach (var playlistPair in listsToProcess) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				BaseItem[] newItems;

				if (playlistPair.SmartPlaylist.MaxItems > 0) {
					newItems = playlistPair.Sorter.GetResults().Take(playlistPair.SmartPlaylist.MaxItems).ToArray();
				}
				else {
					newItems = playlistPair.Sorter.GetResults().ToArray();
				}

				if (playlistPair.Playlist is null) {
					if (string.IsNullOrEmpty(playlistPair.SmartPlaylist.Dto.FileName)) {
						playlistPair.ProcessRunData.UpdatePlaylistRun("Error filename is null, cannot save playlist to disk");
					}
					else {
						CreateNewPlaylist(playlistPair.SmartPlaylist.Dto, newItems);
						_logger.LogInformation("Saving playlist {PlaylistName}", playlistPair.ProcessRunData.FileId);
						SmartPlaylistManager.SavePlaylist(playlistPair.SmartPlaylist.Dto.FileName, playlistPair.SmartPlaylist.Dto);
					}
				}
				else {
					_logger.LogInformation("Clearing and adding {Number} files to existing playlist: {PlaylistName}",
										   newItems.Length,
										   playlistPair.ProcessRunData.FileId);

					playlistPair.Playlist.LinkedChildren = newItems.Select(LinkedChild.Create).ToArray();
					await playlistPair.Playlist.UpdateToRepositoryAsync(ItemUpdateType.MetadataEdit, CancellationToken.None)
							  .ConfigureAwait(false);

					_providerManager.QueueRefresh(
												  playlistPair.Playlist.Id,
												  new (new DirectoryService(_fileSystem)) {
														  ForceSave = true,
														  ImageRefreshMode = MetadataRefreshMode.FullRefresh,
												  },
												  RefreshPriority.High);
                }

				playlistPair.ProcessRunData.UpdatePlaylistRunAsSuccessful(playlistPair.SmartPlaylist.Dto.Id);
			}
			catch (Exception e) {
				playlistPair.ProcessRunData.UpdatePlaylistRun("Error getting results", e);
				playlistPair.HasErrorOccurred = true;
			}
		}

		var pl = _playlistManager.GetPlaylists(_user.Id).ToList();
	}

	private List<PlaylistPair> GetBuiltPlaylists(IEnumerable<PlaylistProcessRunData> playlists) {
		var userLists = GetUserPlaylists().ToArray();
		var pairs     = new List<PlaylistPair>(20);

		foreach (var dto in playlists) {
			if (dto.SmartPlaylist is null) {
				SmartPlaylistManager.UpdatePlaylistRun(dto.FileId, "Playlist is null");
				continue;
			}

			Models.SmartPlaylist pl;
			Playlist? jfList;

			try {
				pl = new (dto.SmartPlaylist);
			}
			catch (Exception ex) {
				SmartPlaylistManager.UpdatePlaylistRun(dto.FileId, "Playlist failed to create", ex);

				_logger.LogError(ex, "Error parsing rules for {FileName}", dto.FileId);

				continue;
			}

			try {
				pl.CompileRules();
			}
			catch (Exception ex) {
				SmartPlaylistManager.UpdatePlaylistRun(dto.FileId, "Playlist failed to compile", ex);

				_logger.LogError(ex, "Error parsing rules for {FileName}", dto.FileId);

				continue;
			}

			try {
				jfList = userLists.FirstOrDefault(x => x.Id.ToString("N") == dto.SmartPlaylist.Id);
			}
			catch (Exception ex) {
				SmartPlaylistManager.UpdatePlaylistRun(dto.FileId, "Error finding the playlist", ex);

				_logger.LogError(ex, "Error finding Jellyfin Playlist {PlaylistName}", dto.SmartPlaylist.Name);

				continue;
			}


			pairs.Add(new(pl, jfList, dto, pl.GetSorter()));
		}

		return pairs;
	}

	private IEnumerable<Playlist> GetUserPlaylists() => _playlistManager.GetPlaylists(_user.Id);

	private void CreateNewPlaylist(SmartPlaylistDto dto, IReadOnlyList<BaseItem> items) {
		var req = new PlaylistCreationRequest {
				Name       = dto.Name,
				UserId     = _user.Id,
				ItemIdList = items.Select(baseItem => baseItem.Id).ToArray(),
		};

		var foo = _playlistManager.CreatePlaylist(req);
		dto.Id = foo.Result.Id;
	}

	private record PlaylistPair(Models.SmartPlaylist        SmartPlaylist,
								Playlist?                   Playlist,
								PlaylistProcessRunData      ProcessRunData,
								Models.SmartPlaylist.Sorter Sorter)
	{
		public bool HasErrorOccurred { get; set; }
	}
}
