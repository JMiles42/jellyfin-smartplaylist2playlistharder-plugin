using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public record PlaylistProcessRunData(SmartPlaylistDto? SmartPlaylist, string FileId, Exception? ErrorDetails)
{
	public PlaylistProcessRunData(SmartPlaylistDto SmartPlaylist, string FileId): this(SmartPlaylist, FileId, null) { }
}
