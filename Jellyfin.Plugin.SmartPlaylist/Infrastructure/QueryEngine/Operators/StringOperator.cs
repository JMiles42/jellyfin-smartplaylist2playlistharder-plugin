namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Operators;

public sealed class StringOperator : IOperator
{
    private static readonly Type[] StringAndComparisonTypeArray = [typeof(string), typeof(StringComparison)];

    private static MethodInfo? GetMethod(SmartPlExpression plExpression, Type parameterPropertyType) =>
            parameterPropertyType.GetMethod(plExpression.Operator, StringAndComparisonTypeArray);

    private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression plExpression,
                                                                              MemberExpression sourceExpression,
                                                                              Type parameterType,
                                                                              MethodInfo method) =>
            plExpression.TargetValue.GetValues()
                        .Select(value => BuildComparisonExpression(plExpression,
                                                                   sourceExpression,
                                                                   parameterType,
                                                                   method,
                                                                   value));

    private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression plExpression,
                                                                         MemberExpression sourceExpression,
                                                                         Type parameterType,
                                                                         MethodInfo method,
                                                                         object value)
    {
        var rightValue = value.ToConstantExpressionAsType(parameterType);
        var stringComparison = LinqExpression.Constant(plExpression.StringComparison);

        var builtExpression = LinqExpression.Call(sourceExpression, method, rightValue, stringComparison);

        return new(builtExpression, plExpression, value);
    }

    /// <inheritdoc />
    public EngineOperatorResult ValidateOperator<T>(SmartPlExpression plExpression,
                                                    MemberExpression sourceExpression,
                                                    ParameterExpression parameterExpression,
                                                    Type parameterPropertyType)
    {
        if (parameterPropertyType != typeof(string))
        {
            return EngineOperatorResult.NotAValidFor("Parameter is not a string");
        }

        var method = GetMethod(plExpression, parameterPropertyType);

        if (method is null)
        {
            return
                    EngineOperatorResult
                            .NotAValidFor($"The operator name {plExpression.Operator} is not a valid method on a string");
        }

        return EngineOperatorResult.Success();
    }

    /// <inheritdoc />
    public ParsedValueExpressions GetOperator<T>(SmartPlExpression plExpression,
                                                 MemberExpression sourceExpression,
                                                 ParameterExpression parameterExpression,
                                                 Type parameterPropertyType)
    {
        var method = GetMethod(plExpression, parameterPropertyType);
        var methodParameterType = method.GetParameters()[0].ParameterType;

        if (plExpression.TargetValue.IsSingleValue)
        {
            return new(plExpression,
                       BuildComparisonExpression(plExpression,
                                                 sourceExpression,
                                                 methodParameterType,
                                                 method,
                                                 plExpression.TargetValue.SingleValue));
        }

        return new(plExpression,
                   GetAllExpressions(plExpression, sourceExpression, methodParameterType, method));
    }
}