namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class ExpressionSet
{
	public List<SmartPlExpression> Expressions { get; set; } = new ();

	public MatchMode Match { get; set; } = MatchMode.All;
}
