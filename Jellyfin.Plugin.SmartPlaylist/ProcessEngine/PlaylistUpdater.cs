using System.Collections.Immutable;
using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Model.Playlists;

namespace Jellyfin.Plugin.SmartPlaylist.ProcessEngine;

public class PlaylistUpdater
{
	private readonly User                _user;
	private readonly BaseItemKind[]      _supportedItems;
	private readonly ILibraryManager     _libraryManager;
	private readonly ILogger             _logger;
	private readonly IPlaylistManager    _playlistManager;
	private readonly ISmartPlaylistStore _plStore;

	public PlaylistUpdater(User             user,
						   BaseItemKind[]   supportedItems,
						   ILibraryManager  libraryManager,
						   IPlaylistManager playlistManager,
						   ISmartPlaylistStore plStore,
						   ILogger          logger) {
		_user            = user;
		_supportedItems  = supportedItems;
		_libraryManager  = libraryManager;
		_logger          = logger;
		_playlistManager = playlistManager;
		_plStore         = plStore;
	}

	private IEnumerable<BaseItem> GetAllUserMedia() {
		var query = new InternalItemsQuery(_user) {
				IncludeItemTypes = _supportedItems,
				Recursive        = true,
		};

		return _libraryManager.GetItemsResult(query).Items;
	}

	public Task ProcessPlaylists(SmartPlaylistDto playlist, Action<double> progressCallback) =>	ProcessPlaylists(new[] { playlist }, progressCallback);
	public async Task ProcessPlaylists(IEnumerable<SmartPlaylistDto> playlists, Action<double> progressCallback) {
		var listsToProcess = GetBuiltPlaylists(playlists);
		var allItems       = GetAllUserMedia();

		foreach (var baseItem in allItems) {
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
				await _plStore.SaveAsync(list.SmartPlaylist.Dto);
			}
			else {
				await _playlistManager.AddToPlaylistAsync(list.Playlist.Id, newItems, _user.Id);
			}
		}
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
