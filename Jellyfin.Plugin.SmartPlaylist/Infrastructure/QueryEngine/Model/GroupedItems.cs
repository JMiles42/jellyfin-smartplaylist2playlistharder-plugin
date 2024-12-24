namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Model;

public sealed class GroupedItems : IEnumerable<SmartPlaylistsRefreshJob>
{
    public GroupedItems(JobGrouping key, IEnumerable<SmartPlaylistsRefreshJob> jobs)
    {
        Key = key;
        Jobs = jobs;
    }

    public JobGrouping Key { get; }

    private IEnumerable<SmartPlaylistsRefreshJob> Jobs { get; }

    /// <inheritdoc />
    public IEnumerator<SmartPlaylistsRefreshJob> GetEnumerator() => Jobs.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Jobs).GetEnumerator();
}