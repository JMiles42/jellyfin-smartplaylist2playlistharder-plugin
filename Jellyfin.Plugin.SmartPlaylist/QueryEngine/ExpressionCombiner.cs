namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

public static class ExpressionCombiner
{
	public static bool DoesMatch(this MatchMode mode, int countMatches, int totalChecks) =>
			mode switch
			{
				MatchMode.Any                      => countMatches > 0,
				MatchMode.All                      => countMatches == totalChecks,
				MatchMode.OnlyOne or MatchMode.One => countMatches == 1,
				MatchMode.AllButOne                => countMatches == (totalChecks - 1),
				MatchMode.None or MatchMode.Zero   => countMatches == 0,
				_                                  => throw new ArgumentOutOfRangeException()
			};
}
