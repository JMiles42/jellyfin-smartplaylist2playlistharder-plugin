using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.RuleFixers;

public interface IExpressionFixer
{
	void FixExpression(SmartPlExpression expression);
}
