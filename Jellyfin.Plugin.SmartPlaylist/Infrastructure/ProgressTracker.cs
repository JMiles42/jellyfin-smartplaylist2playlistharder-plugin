namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class ProgressTracker: IProgress<double>
{
	private readonly IProgress<double> _parent;

	public ProgressTracker(IProgress<double> parent) {
		_parent = parent;
	}

	public int Length { get; set; }
	public int Index  { get; set; }

	/// <inheritdoc />
	public void Report(double value) {
		_parent.ReportPercentage(Length, Index, value);
    }
}
