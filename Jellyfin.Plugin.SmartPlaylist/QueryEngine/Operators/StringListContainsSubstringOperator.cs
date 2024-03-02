using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class StringListContainsSubstringOperator: IOperator {

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
	public Expression GetOperator<T>(SmartPlExpression   plExpression,
									 MemberExpression    sourceExpression,
									 ParameterExpression parameterExpression,
									 Type                parameterPropertyType) {
		if (plExpression.TargetValue.IsSingleValue) {
			return BuildComparisonExpression(plExpression, sourceExpression, plExpression.TargetValue.SingleValue);
		}

		return GetAllExpressions(plExpression, sourceExpression).CombineExpressions(plExpression.Match);
	}

	private static IEnumerable<Expression> GetAllExpressions(SmartPlExpression expression, MemberExpression sourceExpression) {
		foreach (var value in expression.TargetValue.GetValues()) {
			yield return BuildComparisonExpression(expression, sourceExpression, value);
		}
	}

	private static Expression BuildComparisonExpression(SmartPlExpression expression,
														MemberExpression  sourceExpression,
														object            value) {
		var rightValue = value.ToConstantExpressionAsType<string>();
		var stringComparison = Expression.Constant(expression.StringComparison);

		// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
		var exper = Expression.Call(null,
									EngineExtensions.StringArrayContainsSubstringMethodInfo,
									sourceExpression,
									rightValue,
									stringComparison);

		return exper;
	}
}
