using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class TimeSpanOperator: IOperator {

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType) {
		if (!Enum.TryParse(plExpression.Operator, out ExpressionType _)) {
			return EngineOperatorResult.NotAValidFor($"{plExpression.Operator} is not a valid ExpressionType");
		}

		if (plExpression.TargetValue.IsSingleValue) {
			if (!TimeSpan.TryParse(plExpression.TargetValue.SingleValue.ToString(), out var ts)) {
				return EngineOperatorResult.NotAValidFor($"{plExpression.TargetValue.SingleValue} is not a valid TimeSpan");
			}
		}
		else {
			var errors = plExpression.TargetValue.GetValues()
									 .Select(value => {
										 var str = value.ToString();

										 if (!TimeSpan.TryParse(str, out var ts)) {
											 return EngineOperatorResult.NotAValidFor($"{str} is not a valid TimeSpan");
										 }

										 return EngineOperatorResult.Success($"{str} is a valid TimeSpan");


									 })
									 .ToArray();

			if (errors.Any(a => a.Kind is not EngineOperatorResultKind.Success)) {
				return EngineOperatorResult.Error(errors.Where(a => a.Kind is not EngineOperatorResultKind.Success).ToArray());
			}
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
											 parameterPropertyType, plExpression.TargetValue.SingleValue);
		}

		return GetAllExpressions(plExpression,
								 sourceExpression,
								 tBinary,
								 parameterPropertyType).CombineExpressions(plExpression.Match);
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
		return Expression.MakeBinary(tBinary, sourceExpression, rightValue);
	}
}
