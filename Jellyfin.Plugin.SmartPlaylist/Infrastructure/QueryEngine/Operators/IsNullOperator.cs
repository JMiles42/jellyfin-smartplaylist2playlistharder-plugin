namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Operators;

public sealed class IsNullOperator : IOperator
{
    private static readonly ConstantExpression NullExpression = LinqExpression.Constant(null);

    /// <inheritdoc />
    public EngineOperatorResult ValidateOperator<T>(SmartPlExpression plExpression,
                                                    MemberExpression sourceExpression,
                                                    ParameterExpression parameterExpression,
                                                    Type parameterPropertyType)
    {
        if (plExpression.OperatorAsLower is "null" or "isnull")
        {
            return EngineOperatorResult.Success();
        }

        return EngineOperatorResult.NotAValidFor();
    }

    /// <inheritdoc />
    public ParsedValueExpressions GetOperator<T>(SmartPlExpression plExpression,
                                                 MemberExpression sourceExpression,
                                                 ParameterExpression parameterExpression,
                                                 Type parameterPropertyType)
    {
        var builtExpression =
                LinqExpression.MakeBinary(LinqExpressionType.Equal, sourceExpression, NullExpression);

        return new(plExpression, new ParsedValueExpressionResult(builtExpression, plExpression, null!));
    }
}