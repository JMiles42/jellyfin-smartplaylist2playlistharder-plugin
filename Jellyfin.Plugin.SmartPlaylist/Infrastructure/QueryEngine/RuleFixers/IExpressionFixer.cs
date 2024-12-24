namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.RuleFixers;

public interface IExpressionFixer
{
    void FixExpression(SmartPlExpression expression);
}