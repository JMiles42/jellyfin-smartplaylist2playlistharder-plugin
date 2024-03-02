using Jellyfin.Plugin.SmartPlaylist.Models;
using System.Linq.Expressions;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

public record ParsedValueExpressionResult(Expression Expression,
                                    SmartPlExpression SourceExpression,
                                    object Value)
{
    public static ParsedValueExpressionResult Empty = new(Expression.Empty(), SmartPlExpression.Empty, null);
}