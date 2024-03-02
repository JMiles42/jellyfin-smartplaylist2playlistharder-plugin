namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

public record ParsedValueExpressionResult(linqExpression    Expression,
										  SmartPlExpression SourceExpression,
										  object            Value)
{
	public static ParsedValueExpressionResult Empty = new(linqExpression.Empty(), SmartPlExpression.Empty, null);
}
