using System.Globalization;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Model;
using Xunit.Abstractions;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;

public class PercentageCalculatorTests
{
	public ITestOutputHelper Logger { get; }

	public PercentageCalculatorTests(ITestOutputHelper logger) {
		Logger = logger;
	}

	[Fact]
	public async Task TestNestedCalculation()
	{
		var progress = new Progress<double>();

		progress.ProgressChanged += LogValue;

		const int X_TOTAL = 2;
		const int Y_TOTAL = 4;


		for (int x = 0; x < X_TOTAL; x++)
		{
			var pro = new NestedProgressTracker(progress, 1D, X_TOTAL, x, X_TOTAL);
			for (int y = 0; y < Y_TOTAL; y++)
			{
				pro.Report(y, Y_TOTAL);
				await Task.Delay(10);
			}
		}
	}

	private void LogValue(object? sender, double e) => Logger.WriteLine(e.ToString(CultureInfo.InvariantCulture));
}
