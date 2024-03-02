namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

public enum MatchMode
{
	Any,
	All,
	OnlyOne,
	One = OnlyOne,
	AllButOne,
	None,
	Zero = None,
}
