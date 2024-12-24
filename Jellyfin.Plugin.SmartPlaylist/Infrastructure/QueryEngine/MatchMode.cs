namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine;

public enum MatchMode
{
    Any,
    All,
    OnlyOne,
    One = OnlyOne,
    AllButOne,
    None,
    Zero = None,
    Half,
    HalfOrMore,
    HalfOrLess,
}