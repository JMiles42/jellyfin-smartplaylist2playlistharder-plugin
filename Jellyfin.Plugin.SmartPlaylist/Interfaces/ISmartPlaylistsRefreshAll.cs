namespace Jellyfin.Plugin.SmartPlaylist.Interfaces;

public interface ISmartPlaylistsRefreshAll
{
    Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken);
}