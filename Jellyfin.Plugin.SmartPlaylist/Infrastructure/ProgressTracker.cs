namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class ProgressTracker: IProgress<double>
{
	private readonly IProgress<double> _parent;
	private          int               _length;
	private          int               _index;
	private          double            _lastPercentage;
	private readonly object            _locker = new();

	public ProgressTracker(IProgress<double> parent, int length = 0)
	{
		_parent = parent;
		_length = length;
	}

	/// <inheritdoc />
	public void Report(double value)
	{
		if (_parent is ProgressTracker pt)
		{
			ReportToParent(value * (pt._index / (double)pt._length));
		}
		else
		{
			value *= 100;

			if (value < _lastPercentage)
			{
				return;
			}

			_lastPercentage = value;

			lock (_locker)
			{
				ReportToParent(_lastPercentage);
			}
		}
	}

	private void ReportToParent(double value)
	{
		lock (_locker)
		{
			_parent.Report(value);
		}
	}

	public void Report(double index, double length) => Report(index / length);

	public void Increment()
	{
		Interlocked.Increment(ref _index);
		Report(_index, _length);
	}

	public void AddLength(int lengthToAdd) => _length += lengthToAdd;
}
