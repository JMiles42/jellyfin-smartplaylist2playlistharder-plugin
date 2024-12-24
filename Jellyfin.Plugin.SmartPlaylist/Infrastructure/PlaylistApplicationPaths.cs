using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using MediaBrowser.Controller;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public sealed class PlaylistApplicationPaths : IPlaylistApplicationPaths
{
    public PlaylistApplicationPaths(string dataPath, ISmartPlaylistPluginConfiguration smartPlaylistPluginConfiguration)
    {
        PlaylistPath = Path.Combine(dataPath, smartPlaylistPluginConfiguration.PlaylistFolderName);
        PlaylistBackupPath = Path.Combine(dataPath, smartPlaylistPluginConfiguration.PlaylistBackupFolderName);
    }

    public PlaylistApplicationPaths(IServerApplicationPaths serverApplicationPaths,
                                    ISmartPlaylistPluginConfiguration smartPlaylistPluginConfiguration) :
            this(serverApplicationPaths.DataPath, smartPlaylistPluginConfiguration)
    { }

    /// <inheritdoc />
    public string PlaylistPath { get; }

    /// <inheritdoc />
    public string PlaylistBackupPath { get; }

    /// <inheritdoc />
    public void EnsureExists()
    {
        Directory.CreateDirectory(PlaylistPath);
        Directory.CreateDirectory(PlaylistBackupPath);
    }
}