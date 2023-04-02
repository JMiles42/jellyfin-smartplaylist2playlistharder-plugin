using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using MediaBrowser.Controller;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class SmartPlaylistFileSystem : ISmartPlaylistFileSystem
{
    public SmartPlaylistFileSystem(IServerApplicationPaths serverApplicationPaths)
    {
        BasePath = Path.Combine(serverApplicationPaths.DataPath, "smartplaylists");

        Directory.CreateDirectory(BasePath);
    }

    public string BasePath { get; }

    public string GetSmartPlaylistFilePath(string smartPlaylistId) =>
            Directory.GetFiles(BasePath, $"{smartPlaylistId}.json", SearchOption.AllDirectories).First();

    public string[] GetSmartPlaylistFilePaths(string userId) => Directory.GetFiles(BasePath);

    public string[] GetAllSmartPlaylistFilePaths() =>
            Directory.GetFiles(BasePath, "*.json", SearchOption.AllDirectories);

    public string GetSmartPlaylistPath(string userId, string playlistId) =>
            Path.Combine(BasePath, $"{playlistId}.json");
}
