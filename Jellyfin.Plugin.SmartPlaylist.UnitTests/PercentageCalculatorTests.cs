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
		var progress       = new Progress<double>();

		progress.ProgressChanged += LogValue;
		var progressTracer = new ProgressTracker(progress, 10);

		foreach (var x in Enumerable.Range(0, 2))
		{
			progressTracer.Increment();
			await Task.Delay(10);
			foreach (var y in Enumerable.Range(0, 4)) {
				progressTracer.Increment();
				await Task.Delay(10);
			}
		}
	}

	private void LogValue(object? sender, double e) => Logger.WriteLine(e.ToString(CultureInfo.InvariantCulture));
}
