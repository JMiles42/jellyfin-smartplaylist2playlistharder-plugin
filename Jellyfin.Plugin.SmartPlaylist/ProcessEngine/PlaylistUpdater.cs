using System.Collections.Immutable;
using System.Linq;
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
	private readonly IPlaylistManager    _playlistManager;
	private readonly IProviderManager    _providerManager;
	private readonly ISmartPlaylistStore _plStore;
	private readonly IProgress<double>   _progress;

	public PlaylistUpdater(User                user,
						   BaseItemKind[]      supportedItems,
						   IFileSystem         fileSystem,
						   ILibraryManager     libraryManager,
						   IPlaylistManager    playlistManager,
						   IProviderManager    providerManager,
						   ISmartPlaylistStore plStore,
						   ILogger             logger,
						   IProgress<double>   progress) {
		_user            = user;
		_fileSystem      = fileSystem;
		_supportedItems  = supportedItems;
		_libraryManager  = libraryManager;
		_logger          = logger;
		_playlistManager = playlistManager;
		_providerManager = providerManager;
		_plStore         = plStore;
		_progress        = progress;
	}

	private IReadOnlyList<BaseItem> GetAllUserMedia() {
		var query = new InternalItemsQuery(_user) {
				IncludeItemTypes = _supportedItems,
				Recursive        = true,
		};

		return _libraryManager.GetItemsResult(query).Items;
	}

	public Task ProcessPlaylists(SmartPlaylistDto playlist) =>	ProcessPlaylists(new[] { playlist });

	public async Task ProcessPlaylists(IEnumerable<SmartPlaylistDto> playlists) {
		var listsToProcess = GetBuiltPlaylists(playlists);
		var allItems       = GetAllUserMedia();
		var progress       = new ProgressTracker(_progress) {
				Length = allItems.Count
		};

		for (var index = 0; index < allItems.Count; index++) {
			var baseItem = allItems[index];
			progress.Index = index;
			progress.ReportPercentage(allItems.Count, index, 50);
			foreach (var list in listsToProcess) {
				list.Sorter.SortItem(baseItem, _libraryManager, _user);
			}
		}

		foreach (var list in listsToProcess) {
			Guid[] newItems;

			if (list.SmartPlaylist.MaxItems > 0) {
				newItems = list.Sorter.GetResults().Take(list.SmartPlaylist.MaxItems).ToArray();
			}
			else {
				newItems = list.Sorter.GetResults().ToArray();
			}

			if (list.Playlist is null) {
				CreateNewPlaylist(list.SmartPlaylist.Dto, newItems);
				await _plStore.SaveAsync(list.SmartPlaylist.Dto).ConfigureAwait(false);
			}
			else {
				ClearPlaylist(list.Playlist, _user, newItems);
                await _playlistManager.AddToPlaylistAsync(list.Playlist.Id, newItems, _user.Id).ConfigureAwait(false);
			}
		}
	}

	private void ClearPlaylist(Playlist playlist, User user, Guid[] newItems) {
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

	private List<PlaylistPair> GetBuiltPlaylists(IEnumerable<SmartPlaylistDto> playlists) {
		var userLists = GetUserPlaylists().ToImmutableArray();
		var pairs     = new List<PlaylistPair>(20);

		foreach (var dto in playlists) {
			var      pl = new Models.SmartPlaylist(dto);
			Playlist jfList;

			try {
				pl.CompileRules();
			}
			catch (Exception ex) {
				_logger.LogError(ex, "Error parsing rules for {FileName}", pl.FileName);

				continue;
			}

			try {
				jfList = userLists.FirstOrDefault(x => x.Id.ToString("N") == dto.Id);
			}
			catch (Exception ex) {
				_logger.LogError(ex, "Error finding Jellyfin Playlist {PlaylistName}", dto.Name);

				continue;
			}


			pairs.Add(new(pl, jfList, pl.GetSorter()));
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
								Playlist                    Playlist,
								Models.SmartPlaylist.Sorter Sorter);
}
