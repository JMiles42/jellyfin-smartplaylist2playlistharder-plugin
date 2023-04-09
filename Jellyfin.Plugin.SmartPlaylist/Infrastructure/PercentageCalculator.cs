namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public static class PercentageCalculator {
	public static double GetPercentage(int length, int index, double percentThroughIndex) {
		if (percentThroughIndex == 0) {
			return (index * 100) / length;
		}

		var percent = percentThroughIndex * (index + 1);

		return percent / length;
	}

	public static void ReportPercentage(this IProgress<double> progress,
										int length,
										int index,
										double percentThroughIndex) {

		progress.Report(GetPercentage(length, index, percentThroughIndex));
	}
}
