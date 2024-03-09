namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine;

public static class EngineFallbackProcessor
{
    public static ParsedValueExpressions ProcessFallback(SmartPlExpression plExpression,
                                                         Type tProp,
                                                         MemberExpression left)
    {
        var method = tProp.GetMethod(plExpression.Operator);

        if (method is null)
        {
            throw new
                    InvalidOperationException($"Operator '{plExpression.Operator}' is not a valid method to call on MemberName '{plExpression.MemberName}' of type: {tProp}");
        }

        if (plExpression.TargetValue.IsSingleValue)
        {
            return new(plExpression,
                       BuildComparisonExpression(plExpression,
                                                 left,
                                                 method,
                                                 plExpression.TargetValue.SingleValue));
        }

        return new(plExpression, GetAllExpressions(plExpression, left, method));
    }

    private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression plExpression,
                                                                              MemberExpression left,
                                                                              MethodInfo method)
    {
        foreach (var value in plExpression.TargetValue.GetValues())
        {
            yield return BuildComparisonExpression(plExpression, left, method, value);
        }
    }

    private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression plExpression,
                                                                         MemberExpression leftValue,
                                                                         MethodInfo method,
                                                                         object value)
    {
        var tParam = method.GetParameters()[0].ParameterType;
        var rightValue = value.ToConstantExpressionAsType(tParam);

        // use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag)'
        var builtExpression = LinqExpression.Call(leftValue, method, rightValue);

        return new(builtExpression, plExpression, value);
    }
}
