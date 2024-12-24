namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Model;

public record SmartPlaylistsRefreshError(string ErrorPrefix, Exception? Exception = null);