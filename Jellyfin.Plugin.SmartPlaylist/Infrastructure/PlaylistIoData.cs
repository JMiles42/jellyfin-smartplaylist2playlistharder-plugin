using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public record PlaylistIoData(SmartPlaylistDto? SmartPlaylist, string FileId, Exception? ErrorDetails)
{
	public PlaylistIoData(SmartPlaylistDto SmartPlaylist, string FileId): this(SmartPlaylist, FileId, null) { }
}
