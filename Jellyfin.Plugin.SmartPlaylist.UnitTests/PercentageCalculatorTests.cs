using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Xunit.Abstractions;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;

public class PercentageCalculatorTests {
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
}
