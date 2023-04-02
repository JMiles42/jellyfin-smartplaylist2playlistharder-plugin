using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.Interfaces;

public interface ISmartPlaylistStore
{
    Task<SmartPlaylistDto> GetSmartPlaylistAsync(Guid smartPlaylistId);

    Task<SmartPlaylistDto[]> LoadPlaylistsAsync(Guid userId);

    Task<SmartPlaylistDto[]> GetAllSmartPlaylistAsync();

    Task SaveAsync(SmartPlaylistDto dynamickPList);

    void Delete(Guid userId, string smartPlaylistId);
}
