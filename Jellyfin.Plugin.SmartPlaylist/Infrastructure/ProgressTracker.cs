namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class ProgressTracker: IProgress<double>
{
	private readonly IProgress<double> parent;

	public ProgressTracker(IProgress<double> parent) {
		this.parent = parent;
	}

	public int Length { get; set; }

	public int Index { get; set; }

	/// <inheritdoc />
	public void Report(double value) {
		parent.ReportPercentage(Length, Index, value);
	}
}
