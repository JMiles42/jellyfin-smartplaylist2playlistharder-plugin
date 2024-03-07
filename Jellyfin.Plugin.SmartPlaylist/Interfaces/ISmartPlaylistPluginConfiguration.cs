namespace Jellyfin.Plugin.SmartPlaylist.Interfaces;

public interface ISmartPlaylistPluginConfiguration
{
    int PlaylistSorterThreadCount { get; }

    bool PlaylistDetailedErrors { get; }

    bool AlwaysSaveFile { get; }

    bool BackupFileOnSave { get; }

    string PlaylistFolderName { get; }

    string PlaylistBackupFolderName { get; }
}
