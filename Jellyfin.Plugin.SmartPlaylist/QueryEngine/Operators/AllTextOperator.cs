using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class AllTextOperator: IOperator {

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType) {
		if (!parameterPropertyType.IsAssignableTo(typeof(HashSet<string>))) {
			return EngineOperatorResult.NotAValidFor(plExpression.MemberName);
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
														MemberExpression  leftValue,
														object            value) {
		var rightValue = value.ToConstantExpressionAsType<string>();
		var stringComparison = Expression.Constant(expression.StringComparison);

		// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
		return Expression.Call(null,
							   EngineExtensions.StringArrayContainsMethodInfo,
							   leftValue,
							   rightValue,
							   stringComparison);
	}
}
