using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.SmartPlaylist.ScheduledTask;

public sealed class RefreshAllSmartPlaylistsScheduledTask : IScheduledTask, IConfigurableScheduledTask
{
    private readonly ISmartPlaylistsRefreshAll _smartPlaylistsRefresh;
    private readonly ILogger _logger;

    public RefreshAllSmartPlaylistsScheduledTask(ISmartPlaylistsRefreshAll smartPlaylistsRefresh,
                                                 ILogger<RefreshAllSmartPlaylistsScheduledTask> logger)
    {
        _smartPlaylistsRefresh = smartPlaylistsRefresh;
        _logger = logger;
    }

    public bool IsHidden => false;

    public bool IsEnabled => true;

    public bool IsLogged => true;

    public string Key => "RefreshAllSmartPlaylists";

    public string Name => "Refresh all SmartPlaylist";

    public string Description => "Refresh all SmartPlaylists Playlists";

    public string Category => "Smart Playlist 2 Playlist Harder";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() =>
    [
        new TaskTriggerInfo
                {
                    IntervalTicks = TimeSpan.FromHours(6).Ticks, Type = TaskTriggerInfoType.IntervalTrigger,
                }
    ];

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            await _smartPlaylistsRefresh.ExecuteAsync(progress, cancellationToken);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {JobName}", nameof(RefreshAllSmartPlaylistsScheduledTask));
        }
    }
}