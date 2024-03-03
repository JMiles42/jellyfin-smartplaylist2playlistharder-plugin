namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class ProgressTracker: IProgress<double>
{
	private readonly IProgress<double> _parent;
	public           int               Length         { get; private set; }
	public           int               Index          { get; private set; }
	public           double            LastPercentage { get; private set; }

	public ProgressTracker(IProgress<double> parent, int length = 0)
	{
		_parent = parent;
		Length  = length;
	}

	/// <inheritdoc />
	public void Report(double value)
	{
		if (_parent is ProgressTracker pt)
		{
			_parent.Report(value * (pt.Index / (double)pt.Length));
		}
		else
		{
			value *= 100;

			if (value < LastPercentage)
			{
				return;
			}
			LastPercentage = value;

			Console.WriteLine(LastPercentage);
			_parent.Report(LastPercentage);
		}
	}

	public void Report(double index, double length) => Report(index / length);

	public void Increment() => Report(Index++, Length);

	public void AddLength(int lengthToAdd) => Length += lengthToAdd;
}
