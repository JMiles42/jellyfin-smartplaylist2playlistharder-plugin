namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public static class PercentageCalculator {
	public static double GetPercentage(int length, int index, double percentThroughIndex) {



		double percent = (index + 1);
		percent /= length;
		percent =  (percentThroughIndex * percent);
		percent *= 100;

		return percent;
	}

	public static void ReportPercentage(this IProgress<double> progress,
										int length,
										int index,
										double percentThroughIndex) {

		progress.Report(GetPercentage(length, index, percentThroughIndex));
	}
}
