using System.Globalization;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Xunit.Abstractions;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;

public class PercentageCalculatorTests
{
	public ITestOutputHelper Logger { get; }

	public PercentageCalculatorTests(ITestOutputHelper logger) {
		Logger = logger;
	}


	[Theory]
	[InlineData(1, 0, 0, 0)]
	[InlineData(1, 0, 1, 100)]
	[InlineData(1, 0, 0.5, 50)]
	[InlineData(1, 0, 0.25, 25)]
	[InlineData(1, 0, 0.75, 75)]
	[InlineData(10, 4, 1, 50)]
	public void TestCalculation(int length, int index, double percentThroughIndex, double result) {
		var percent = PercentageCalculator.GetPercentage(length, index, percentThroughIndex);
		Logger.WriteLine(percent.ToString());
		Assert.Equal(result, percent);
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
