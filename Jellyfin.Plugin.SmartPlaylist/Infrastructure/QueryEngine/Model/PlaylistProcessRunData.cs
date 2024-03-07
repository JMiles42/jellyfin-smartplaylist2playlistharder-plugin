namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Model;

public record PlaylistProcessRunData(SmartPlaylistDto? SmartPlaylist, string FileId, Exception? ErrorDetails)
{
    public PlaylistProcessRunData(SmartPlaylistDto SmartPlaylist, string FileId) : this(SmartPlaylist, FileId, null) { }
}
