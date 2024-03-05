namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

public record ParsedValueExpressionResult(LinqExpression    Expression,
										  SmartPlExpression SourceExpression,
										  object            Value)
{
	public static ParsedValueExpressionResult Empty = new(LinqExpression.Empty(), SmartPlExpression.Empty, null);
}
