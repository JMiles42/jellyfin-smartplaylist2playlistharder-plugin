namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Model;

public record SmartPlaylistLastRunDetails(string                            PlaylistId,
										  string                            Status,
										  List<SmartPlaylistsRefreshError>? Errors             = null,
										  Guid?                             JellyfinPlaylistId = null)
{
	public const string SUCCESS = "Success";
	public const string ERRORED = "Errored";
}
