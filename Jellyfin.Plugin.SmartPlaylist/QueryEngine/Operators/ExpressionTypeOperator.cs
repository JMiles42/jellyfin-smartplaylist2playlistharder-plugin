using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class ExpressionTypeOperator: IOperator {

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType) {
		if (!Enum.TryParse(plExpression.Operator, out ExpressionType _)) {
			return EngineOperatorResult.NotAValidFor($"{plExpression.Operator} is not a valid ExpressionType");
		}

		return EngineOperatorResult.Success();
	}

	/// <inheritdoc />
	public Expression GetOperator<T>(SmartPlExpression   plExpression,
									 MemberExpression    sourceExpression,
									 ParameterExpression parameterExpression,
									 Type                parameterPropertyType) {
		Enum.TryParse(plExpression.Operator, out ExpressionType tBinary);


		if (plExpression.TargetValue.IsSingleValue) {
			return BuildComparisonExpression(plExpression,
											 sourceExpression,
											 tBinary,
											 parameterPropertyType,
											 plExpression.TargetValue.SingleValue);
		}

		return GetAllExpressions(plExpression,
								 sourceExpression,
								 tBinary,
								 parameterPropertyType)
					.CombineExpressions(plExpression.Match);
	}

	private static IEnumerable<Expression> GetAllExpressions(SmartPlExpression expression,
															 MemberExpression  sourceExpression,
															 ExpressionType    tBinary,
															 Type              propertyType) {
		foreach (var value in expression.TargetValue.GetValues()) {
			yield return BuildComparisonExpression(expression, sourceExpression, tBinary, propertyType, value);
		}
	}

	private static Expression BuildComparisonExpression(SmartPlExpression expression,
														MemberExpression  sourceExpression,
														ExpressionType    tBinary,
														Type              propertyType,
														object            value) {
		var rightValue = value.ToConstantExpressionAsType(propertyType);

		// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
		var exper = Expression.MakeBinary(tBinary, sourceExpression, rightValue);

		return exper;
	}
}
