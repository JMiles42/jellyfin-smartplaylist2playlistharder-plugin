using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class StringListContainsSubstringOperator: IOperator
{

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType) {
		if (plExpression.OperatorAsLower is not "stringlistcontainssubstring") {
			return EngineOperatorResult.NotAValidFor(nameof(StringListContainsSubstringOperator));
		}

		if (!parameterPropertyType.IsAssignableTo(typeof(IReadOnlyCollection<string>))) {

			return EngineOperatorResult.Error($"Selected property {plExpression.MemberName} is not a string list");
		}

		return EngineOperatorResult.Success();
	}

	/// <inheritdoc />
	public ParsedValueExpressions GetOperator<T>(SmartPlExpression   plExpression,
											 MemberExpression    sourceExpression,
											 ParameterExpression parameterExpression,
											 Type                parameterPropertyType) {
		if (plExpression.TargetValue.IsSingleValue) {
			return new(plExpression.Match,
					   BuildComparisonExpression(plExpression,
												 sourceExpression,
												 plExpression.TargetValue.SingleValue));
		}

		return new(plExpression.Match,
				   GetAllExpressions(plExpression, sourceExpression));
	}

	private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression expression,
																		MemberExpression  sourceExpression) {
		foreach (var value in expression.TargetValue.GetValues()) {
			yield return BuildComparisonExpression(expression, sourceExpression, value);
		}
	}

	private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression plExpression,
																   MemberExpression  sourceExpression,
																   object            value) {
		var rightValue       = value.ToConstantExpressionAsType<string>();
		var stringComparison = Expression.Constant(plExpression.StringComparison);

		// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
		var builtExpression = Expression.Call(null,
											  EngineExtensions.StringArrayContainsSubstringMethodInfo,
											  sourceExpression,
											  rightValue,
											  stringComparison);

		return new(builtExpression, plExpression, value);
	}
}
