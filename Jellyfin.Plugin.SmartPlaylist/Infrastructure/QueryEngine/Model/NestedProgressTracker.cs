namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Model;

public class NestedProgressTracker: IProgress<double>
{
	private readonly IProgress<double> _parent;

	private readonly double _multiplier;

	private readonly double _currentProgress;

	public NestedProgressTracker(IProgress<double> parent, double multiplier = 1, double currentProgress = 0)
	{
		_parent          = parent;
		_multiplier      = multiplier;
		_currentProgress = currentProgress;
	}

	public NestedProgressTracker(IProgress<double> parent,
								 double            multiplierSize        = 1,
								 double            multiplierLength      = 100,
								 double            currentProgressIndex  = 0,
								 double            currentProgressLength = 1)
	{
		_parent          = parent;
		_multiplier      = (multiplierSize / multiplierLength);
		_currentProgress = (currentProgressIndex / currentProgressLength) * 100;
	}

	public void Report(double value) => _parent.Report((value * _multiplier) + _currentProgress);

	public void Report(double index, double length) =>
			_parent.Report((100 * ((index + 1D) / length) * _multiplier) + _currentProgress);
}
