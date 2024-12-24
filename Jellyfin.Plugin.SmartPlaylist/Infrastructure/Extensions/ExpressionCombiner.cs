namespace Jellyfin.Plugin.SmartPlaylist.Extensions;

public static class ExpressionCombiner
{
    public static bool DoesMatch(this MatchMode mode, int countMatches, int totalChecks)
    {
        if (totalChecks == 0)
        {
            return false;
        }

        //If we haven't matched anything, generally it should fail.
        //So return false, unless the mode is None or Zero
        if (countMatches == 0)
        {
            return mode is MatchMode.None or MatchMode.Zero;
        }

        return mode switch
        {
            MatchMode.Any => countMatches > 0,
            MatchMode.All => countMatches == totalChecks,
            MatchMode.OnlyOne or MatchMode.One => countMatches == 1,
            MatchMode.AllButOne => countMatches == (totalChecks - 1),
            MatchMode.None or MatchMode.Zero => false,
            MatchMode.Half => countMatches == (totalChecks / 2),
            MatchMode.HalfOrMore => countMatches >= (totalChecks / 2),
            MatchMode.HalfOrLess => countMatches <= (totalChecks / 2),
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };
    }
}