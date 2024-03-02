using System.Linq.Expressions;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class AllTextOperator: IOperator
{

	private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression expression,
																			  MemberExpression  sourceExpression)
	{
		foreach (var value in expression.TargetValue.GetValues())
		{
			yield return BuildComparisonExpression(expression, sourceExpression, value);
		}
	}

	private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression expression,
																		 MemberExpression  leftValue,
																		 object            value)
	{
		var rightValue       = value.ToConstantExpressionAsType<string>();
		var stringComparison = Expression.Constant(expression.StringComparison);

		// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
		var builtExpression = Expression.Call(null,
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
