using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public static class SmartPlaylistManager
{
	private static ConcurrentDictionary<string, SmartPlaylistLastRunDetails> Data { get; } = new();

	public static SmartPlaylistLastRunDetails[] GetAllRunDetails() => Data.Values.ToArray();

	public static IEnumerable<PlaylistProcessRunData> GetAllValidPlaylists(string folderLocation) {
		var all = LoadAllPlaylists(folderLocation).ToArray();

		CleanOldJobs(all);

		foreach (var playlistIoData in all) {
			if (playlistIoData.ErrorDetails is null) {
				yield return playlistIoData;
			}
			else {
				UpdatePlaylistRun(playlistIoData.FileId, "Parsing Error", playlistIoData.ErrorDetails);
			}
		}
	}

	private static void CleanOldJobs(PlaylistProcessRunData[] allPlaylists) {
		var hash = new HashSet<string>(allPlaylists.Select(a => a.FileId));

		var allLoadedIds = Data.Keys.ToArray();

		foreach (var allLoadedId in allLoadedIds) {
			if (!hash.Contains(allLoadedId)) {
				Data.TryRemove(allLoadedId, out _);
			}
		}
	}

	public static void UpdatePlaylistRun(string fileId, string statusOrErrorPrefix, Exception? exception = null, string? jellyfinPlaylistId = null) {
		SmartPlaylistLastRunDetails runData = new(fileId, statusOrErrorPrefix, exception, jellyfinPlaylistId);
		Data[fileId] = runData;
	}

	public static IEnumerable<PlaylistProcessRunData> LoadAllPlaylists(string folderLocation) {
		return Directory.EnumerateFiles(folderLocation, "*.json", SearchOption.AllDirectories)
						.Select(file => {
							var fileId = file[folderLocation.Length..].TrimStart('/').TrimStart('\\');

							return LoadPlaylist(file, fileId);
						});
	}

	public static PlaylistProcessRunData LoadPlaylist(string filepath, string fileId) {
		SmartPlaylistDto? playlist  = null;
		Exception?        exception = null;

		try {
			using var reader = File.OpenRead(filepath);

			playlist = JsonSerializer.Deserialize(reader, SmartPlaylistDtoJsonContext.WithConverters.SmartPlaylistDto);

			if (playlist is not null) {
				playlist.Validate();
				playlist.FileName = filepath;
			}
		}
		catch (Exception e) {
			exception = e;
		}

		return new(playlist, fileId, exception);
	}

	public static void SavePlaylist(string file, SmartPlaylistDto dto) {
		if (File.Exists(file)) {
			File.Delete(file);
		}

		using var writer = File.OpenWrite(file);

		JsonSerializer.Serialize(writer, dto, SmartPlaylistDtoJsonContext.WithConverters.SmartPlaylistDto);
	}

	public static void UpdatePlaylistRun(this PlaylistProcessRunData data, string statusOrErrorPrefix, Exception? exception = null) => UpdatePlaylistRun(data.FileId, statusOrErrorPrefix, exception);

	public static void UpdatePlaylistRunAsSuccessful(this PlaylistProcessRunData data, string? jellyfinPlaylistId = null) => UpdatePlaylistRun(data.FileId, SmartPlaylistLastRunDetails.SUCCESS, null, jellyfinPlaylistId);
}
