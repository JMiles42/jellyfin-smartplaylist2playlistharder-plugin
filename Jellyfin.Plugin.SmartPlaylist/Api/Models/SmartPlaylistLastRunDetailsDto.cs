namespace Jellyfin.Plugin.SmartPlaylist.Api.Models;

public record SmartPlaylistLastRunDetailsDto(string PlaylistId,
                                             string Status,
                                             IEnumerable<string>? Errors = null,
                                             Guid? JellyfinPlaylistId = null);