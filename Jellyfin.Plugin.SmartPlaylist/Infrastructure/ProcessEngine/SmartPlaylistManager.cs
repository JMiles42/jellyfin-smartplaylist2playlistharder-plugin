using Jellyfin.Plugin.SmartPlaylist.Interfaces;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.ProcessEngine;

public interface ISmartPlaylistManager
{
    IEnumerable<PlaylistProcessRunData> GetAllPlaylists();

    IEnumerable<PlaylistProcessRunData> GetAllPlaylists(string folderLocation);

    IEnumerable<PlaylistProcessRunData> LoadAllPlaylists(string folderLocation);

    PlaylistProcessRunData LoadPlaylist(string fileId);

    void SavePlaylist(string file, SmartPlaylistDto dto);
}

public class SmartPlaylistManager: ISmartPlaylistManager {
    private readonly IPlaylistApplicationPaths         _paths;
    private readonly ISmartPlaylistPluginConfiguration _config;

    private static ConcurrentDictionary<string, SmartPlaylistLastRunDetails> Data { get; } = new();

    public static SmartPlaylistLastRunDetails[] GetAllRunDetails() => Data.Values.ToArray();


    public SmartPlaylistManager(IPlaylistApplicationPaths paths,
                                ISmartPlaylistPluginConfiguration config)
    {
        _paths   = paths;
        _config = config;
    }


    public IEnumerable<PlaylistProcessRunData> GetAllPlaylists() => GetAllPlaylists(_paths.PlaylistPath);

    public IEnumerable<PlaylistProcessRunData> GetAllPlaylists(string folderLocation)
    {
        var all = LoadAllPlaylists(folderLocation).ToArray();

        CleanOldJobs(all);

        foreach (var playlistIoData in all)
        {
            yield return playlistIoData;
        }
    }

    private void CleanOldJobs(PlaylistProcessRunData[] allPlaylists)
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

    public IEnumerable<PlaylistProcessRunData> LoadAllPlaylists(string folderLocation) =>
            Directory.EnumerateFiles(folderLocation, "*.json", SearchOption.AllDirectories)
                     .Select(file =>
                     {
                         var fileId = file[folderLocation.Length..].TrimStart('/').TrimStart('\\');

                         return LoadPlaylist(fileId);
                     });

    public PlaylistProcessRunData LoadPlaylist(string fileId)
    {
        var               filepath  = Path.Combine(_paths.PlaylistPath, fileId);
        SmartPlaylistDto? playlist  = null;
        Exception?        exception = null;

        try
        {
            using var reader = File.OpenRead(filepath);

            playlist = JsonSerializer.Deserialize(reader, SmartPlaylistDtoJsonContext.WithConverters.SmartPlaylistDto);

            if (playlist is not null)
            {
                playlist.FileName = filepath;
            }
        }
        catch (Exception e)
        {
            exception = e;
        }

        return new(playlist, fileId, exception);
    }

    public void SavePlaylist(string filename, SmartPlaylistDto dto)
    {
        _paths.EnsureExists();
        var file = Path.Combine(_paths.PlaylistPath, filename);

        if (_config.BackupFileOnSave)
        {
            var dir = Path.GetDirectoryName(file);

            var backupFolder = Path.Combine(_paths.PlaylistBackupPath, $"{DateTime.Now:yyyy-MM-dd_hhmmss}");

            var backupName = $"{file}.pl.bkup";

            if (File.Exists(file))
            {
                File.Move(file, backupName);
            }
        }

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        using var writer = File.OpenWrite(file);

        JsonSerializer.Serialize(writer, dto, SmartPlaylistDtoJsonContext.WithConverters.SmartPlaylistDto);
    }

    public static void SetErrorStatus(string jobFileId,
                                      string status = SmartPlaylistLastRunDetails.ERRORED,
                                      List<SmartPlaylistsRefreshError>? jobProcessErrors = null,
                                      Guid? jellyfinPlaylistId = null)
    {
        jobProcessErrors ??= new();

        Data[jobFileId] = new(jobFileId, status, jobProcessErrors, jellyfinPlaylistId);
    }

    public static void SetStatus(string jobFileId,
                                 string status = SmartPlaylistLastRunDetails.SUCCESS,
                                 Guid? jellyfinPlaylistId = null) =>
            Data[jobFileId] = new(jobFileId, status, JellyfinPlaylistId: jellyfinPlaylistId);
}
