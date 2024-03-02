namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class ProgressTracker: IProgress<double>
{
	private readonly IProgress<double> _parent;

	public int Length { get; set; }

	public int Index { get; set; }

	public ProgressTracker(IProgress<double> parent) => _parent = parent;

	/// <inheritdoc />
	public void Report(double value) => _parent.ReportPercentage(Length, Index, value);
}
