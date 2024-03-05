namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Model;

public record SmartPlaylistsRefreshError(string ErrorPrefix, Exception? Exception = null);
