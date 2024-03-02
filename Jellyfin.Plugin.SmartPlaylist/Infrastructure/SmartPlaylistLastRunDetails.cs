namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public record SmartPlaylistLastRunDetails(string PlaylistId, string StatusOrErrorPrefix, Exception? Exception = null, string? JellyfinPlaylistId = null)
{
	public const string SUCCESS = "Success";
}
