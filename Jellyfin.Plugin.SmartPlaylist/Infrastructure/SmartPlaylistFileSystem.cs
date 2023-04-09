using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using MediaBrowser.Controller;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class SmartPlaylistFileSystem : ISmartPlaylistFileSystem {
	public string SmartPlaylistsPath { get; }
	public SmartPlaylistFileSystem(IServerApplicationPaths serverApplicationPaths) {
		SmartPlaylistsPath = Path.Combine(serverApplicationPaths.DataPath, "smartplaylists");

		Directory.CreateDirectory(SmartPlaylistsPath);
	}

	public string GetSmartPlaylistFilePath(string smartPlaylistId) => Directory.GetFiles(SmartPlaylistsPath, $"{smartPlaylistId}.json", SearchOption.AllDirectories).First();

	public string[] GetSmartPlaylistFilePaths(string userId) => Directory.GetFiles(SmartPlaylistsPath);

	public string[] GetAllSmartPlaylistFilePaths() => Directory.GetFiles(SmartPlaylistsPath, "*.json", SearchOption.AllDirectories);

	public string GetSmartPlaylistPath(string userId, string playlistId) => Path.Combine(SmartPlaylistsPath, $"{playlistId}.json");
}
