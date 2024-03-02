namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public static class SmartPlaylistManager
{
	private static ConcurrentDictionary<string, SmartPlaylistLastRunDetails> Data { get; } = new();

	public static SmartPlaylistLastRunDetails[] GetAllRunDetails() => Data.Values.ToArray();

	public static IEnumerable<PlaylistProcessRunData> GetAllPlaylists(string folderLocation)
	{
		var all = LoadAllPlaylists(folderLocation).ToArray();

		CleanOldJobs(all);

		foreach (var playlistIoData in all)
		{
			yield return playlistIoData;
		}
	}

	private static void CleanOldJobs(PlaylistProcessRunData[] allPlaylists)
	{
		var hash = new HashSet<string>(allPlaylists.Select(a => a.FileId));

		var allLoadedIds = Data.Keys.ToArray();

		foreach (var allLoadedId in allLoadedIds)
		{
			if (!hash.Contains(allLoadedId))
			{
				Data.TryRemove(allLoadedId, out _);
			}
		}
	}

	public static IEnumerable<PlaylistProcessRunData> LoadAllPlaylists(string folderLocation) =>
			Directory.EnumerateFiles(folderLocation, "*.json", SearchOption.AllDirectories)
					 .Select(file =>
					 {
						 var fileId = file[folderLocation.Length..].TrimStart('/').TrimStart('\\');

						 return LoadPlaylist(file, fileId);
					 });

	public static PlaylistProcessRunData LoadPlaylist(string filepath, string fileId)
	{
		SmartPlaylistDto? playlist  = null;
		Exception?        exception = null;

		try
		{
			using var reader = File.OpenRead(filepath);

			playlist = JsonSerializer.Deserialize(reader, SmartPlaylistDtoJsonContext.WithConverters.SmartPlaylistDto);

			if (playlist is not null)
			{
				playlist.Validate();
				playlist.FileName = filepath;
			}
		}
		catch (Exception e)
		{
			exception = e;
		}

		return new(playlist, fileId, exception);
	}

	public static void SavePlaylist(string file, SmartPlaylistDto dto)
	{
		if (File.Exists(file))
		{
			File.Delete(file);
		}

		using var writer = File.OpenWrite(file);

		JsonSerializer.Serialize(writer, dto, SmartPlaylistDtoJsonContext.WithConverters.SmartPlaylistDto);
	}

	public static void SetErrorStatus(string                            jobFileId,
									  string                            status = SmartPlaylistLastRunDetails.ERRORED,
									  List<SmartPlaylistsRefreshError>? jobProcessErrors = null,
									  Guid?                             jellyfinPlaylistId = null)
	{
		jobProcessErrors ??= new();

		Data[jobFileId] = new(jobFileId, status, jobProcessErrors, jellyfinPlaylistId);
	}

	public static void SetStatus(string jobFileId,
								 string status             = SmartPlaylistLastRunDetails.SUCCESS,
								 Guid?  jellyfinPlaylistId = null) =>
			Data[jobFileId] = new(jobFileId, status, JellyfinPlaylistId: jellyfinPlaylistId);
}
