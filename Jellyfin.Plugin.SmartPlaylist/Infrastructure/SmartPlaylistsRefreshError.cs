namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public record SmartPlaylistsRefreshError(string ErrorPrefix, Exception? Exception = null);
