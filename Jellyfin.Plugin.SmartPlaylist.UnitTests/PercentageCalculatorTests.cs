using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Xunit.Abstractions;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;

public class PercentageCalculatorTests {
	public ITestOutputHelper Logger { get; }

	public PercentageCalculatorTests(ITestOutputHelper logger) {
		Logger = logger;
	}


	[Theory]
	[InlineData(4, 3, 100, 100)]
	[InlineData(4, 3, 0, 75)]
	[InlineData(4, 2, 0, 50)]
	[InlineData(4, 1, 0, 25)]
	[InlineData(4, 0, 0, 00)]
	[InlineData(1, 0, 100, 100)]
	[InlineData(1, 0, 50, 50)]
	[InlineData(1, 0, 0, 0)]
	public void TestCalculation(int length, int index, double percentThroughIndex, double result) {
		var percent = PercentageCalculator.GetPercentage(length, index, percentThroughIndex);
		Logger.WriteLine(percent.ToString());
		Assert.Equal(result, percent);
	}
}
