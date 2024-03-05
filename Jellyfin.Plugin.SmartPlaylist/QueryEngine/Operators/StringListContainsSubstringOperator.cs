namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class StringListContainsSubstringOperator: IOperator
{

	private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression expression,
																			  MemberExpression  sourceExpression) =>
			expression.TargetValue.GetValues()
					  .Select(value => BuildComparisonExpression(expression, sourceExpression, value));

	private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression plExpression,
																		 MemberExpression  sourceExpression,
																		 object            value)
	{
		var rightValue       = value.ToConstantExpressionAsType<string>();
		var stringComparison = LinqExpression.Constant(plExpression.StringComparison);

		var builtExpression = LinqExpression.Call(null,
												  EngineExtensions.StringArrayContainsSubstringMethodInfo,
												  sourceExpression,
												  rightValue,
												  stringComparison);

		return new(builtExpression, plExpression, value);
	}

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType)
	{
		if (plExpression.OperatorAsLower is not "stringlistcontainssubstring")
		{
			return EngineOperatorResult.NotAValidFor(nameof(StringListContainsSubstringOperator));
		}

		if (!parameterPropertyType.IsAssignableTo(typeof(IReadOnlyCollection<string>)))
		{
			return EngineOperatorResult.Error($"Selected property {plExpression.MemberName} is not a string list");
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
				   GetAllExpressions(plExpression, sourceExpression));
	}
}
