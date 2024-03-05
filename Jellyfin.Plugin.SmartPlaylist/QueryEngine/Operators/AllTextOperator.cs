namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class AllTextOperator: IOperator
{

	private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression expression,
																			  MemberExpression  sourceExpression) =>
			expression.TargetValue.GetValues()
					  .Select(value => BuildComparisonExpression(expression, sourceExpression, value));

	private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression expression,
																		 MemberExpression  leftValue,
																		 object            value)
	{
		var rightValue       = value.ToConstantExpressionAsType<string>();
		var stringComparison = LinqExpression.Constant(expression.StringComparison);

		var builtExpression = LinqExpression.Call(null,
												  EngineExtensions.StringArrayContainsSubstringMethodInfo,
												  leftValue,
												  rightValue,
												  stringComparison);

		return new(builtExpression, expression, value);
	}

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType)
	{
		if (!parameterPropertyType.IsAssignableTo(typeof(HashSet<string>)))
		{
			return EngineOperatorResult.NotAValidFor(plExpression.MemberName);
		}

		return EngineOperatorResult.Success();
	}

	/// <inheritdoc />
	public ParsedValueExpressions GetOperator<T>(SmartPlExpression   plExpression,
												 MemberExpression    sourceExpression,
												 ParameterExpression parameterExpression,
												 Type                parameterPropertyType)
	{
		if (plExpression.TargetValue.IsSingleValue)
		{
			return new(plExpression.Match,
					   BuildComparisonExpression(plExpression,
												 sourceExpression,
												 plExpression.TargetValue.SingleValue));
		}

		return new(plExpression.Match,
				   GetAllExpressions(plExpression,
									 sourceExpression));
	}
}
