namespace Jellyfin.Plugin.SmartPlaylist.Interfaces;

public interface IPlaylistApplicationPaths
{
    string PlaylistPath { get; }
    string PlaylistBackupPath { get; }

    void EnsureExists();
}