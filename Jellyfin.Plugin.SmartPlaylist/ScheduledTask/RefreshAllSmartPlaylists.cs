using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.SmartPlaylist.ScheduledTask;

public class RefreshAllSmartPlaylistsScheduledTask: IScheduledTask, IConfigurableScheduledTask
{
	private readonly SmartPlaylistsRefreshAll _smartPlaylistsRefresh;
	private readonly ILogger                  _logger;

	public RefreshAllSmartPlaylistsScheduledTask(SmartPlaylistsRefreshAll                       smartPlaylistsRefresh,
												 ILogger<RefreshAllSmartPlaylistsScheduledTask> logger)
	{
		_smartPlaylistsRefresh = smartPlaylistsRefresh;
		_logger                = logger;
	}

	public bool IsHidden => false;

	public bool IsEnabled => true;

	public bool IsLogged => true;

	public string Key => "RefreshAllSmartPlaylists";

	public string Name => "Refresh all SmartPlaylist";

	public string Description => "Refresh all SmartPlaylists Playlists";

	public string Category => "Smart Playlist 2 Playlist Harder";

	// TODO check for creation of schedule json file. Isn't created currently and won't execute until it is.
	public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() =>
			new[]
			{
				new TaskTriggerInfo
				{
					IntervalTicks = TimeSpan.FromHours(6).Ticks, Type = TaskTriggerInfo.TriggerInterval,
				},
			};

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
