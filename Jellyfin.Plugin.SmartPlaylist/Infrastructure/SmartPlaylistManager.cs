using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public static class SmartPlaylistManager
{
	private static ConcurrentDictionary<string, SmartPlaylistLastRunDetails> Data { get; } = new();

	public static SmartPlaylistLastRunDetails[] GetAllRunDetails() => Data.Values.ToArray();

	public static IEnumerable<PlaylistIoData> GetAllValidPlaylists(string folderLocation) {
		var all = LoadAllPlaylists(folderLocation).ToArray();

		CleanOldJobs(all);

		foreach (var playlistIoData in all) {
			if (playlistIoData.ErrorDetails is null) {
				yield return playlistIoData;
			}
			else {
				UpdatePlaylistRun(playlistIoData.FileId, playlistIoData.ErrorDetails.Message);
			}
		}
	}

	private static void CleanOldJobs(PlaylistIoData[] allPlaylists) {
		var hash = new HashSet<string>(allPlaylists.Select(a => a.FileId));

		var allLoadedIds = Data.Keys.ToArray();

		foreach (var allLoadedId in allLoadedIds) {
			if (!hash.Contains(allLoadedId)) {
				Data.TryRemove(allLoadedId, out _);
			}
		}
	}

	public static void UpdatePlaylistRun(string fileId, string messageText) {
		SmartPlaylistLastRunDetails runData = new(fileId, messageText);
		Data[fileId] = runData;
	}

	public static IEnumerable<PlaylistIoData> LoadAllPlaylists(string folderLocation) {
		foreach (var file in Directory.EnumerateFiles(folderLocation, "*.json", SearchOption.AllDirectories)) {
			yield return LoadPlaylist(folderLocation, file);
		}
	}

	public static PlaylistIoData LoadPlaylist(string folderLocation, string file) {
		SmartPlaylistDto? playlist  = null;
		Exception         exception = null;
		var               fileId    = file;
		var filepath = Path.Combine(folderLocation, file);

		try {
			using var reader = File.OpenRead(filepath);

			playlist = JsonSerializer.Deserialize(reader, SmartPlaylistDtoJsonContext.WithConverters.SmartPlaylistDto);
			playlist?.Validate();
		}
		catch (Exception e) {
			exception = e;
		}

		return new(playlist, fileId, exception);
	}

	public static void SavePlaylist(string file, SmartPlaylistDto dto) {
		using var writer = File.OpenWrite(file);

		JsonSerializer.Serialize(writer, dto, SmartPlaylistDtoJsonContext.WithConverters.SmartPlaylistDto);
	}


	public static void UpdatePlaylistRun(this PlaylistIoData data, string message) => UpdatePlaylistRun(data.FileId, message);
}