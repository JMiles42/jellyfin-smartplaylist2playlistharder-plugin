using System.Linq.Expressions;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

public static class ExpressionCombiner
{
    public static Expression CombineExpressions(this IEnumerable<Expression> expressions, MatchMode matchMode)
    {
        var array = expressions.ToArray().AsSpan();

        return matchMode switch
        {
            MatchMode.All => And(array),
            MatchMode.AllButOne => AllButOne(array),
            MatchMode.OnlyOne => OnlyOne(array),
            MatchMode.Any => Any(array),
            _ => Any(array)
        };
    }

    private static Expression And(Span<Expression> expressions)
    {
        return expressions.Length switch
        {
            1 => expressions[0],
            2 => Expression.AndAlso(expressions[0], expressions[1]),
            _ => Expression.AndAlso(expressions[0], And(expressions[1..^1]))
        };
    }

    private static Expression Any(Span<Expression> expressions)
    {
        return expressions.Length switch
        {
            1 => expressions[0],
            2 => Expression.OrElse(expressions[0], expressions[1]),
            _ => Expression.OrElse(expressions[0], And(expressions[1..^1]))
        };
    }

    private static Expression OnlyOne(Span<Expression> array) => throw new NotImplementedException();

    private static Expression AllButOne(Span<Expression> array) => throw new NotImplementedException();
}
