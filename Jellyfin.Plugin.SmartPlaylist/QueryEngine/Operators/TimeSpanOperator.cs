using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class TimeSpanOperator: IOperator
{

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType) {
		if (parameterPropertyType != typeof(TimeSpan)) {
			return EngineOperatorResult.NotAValidFor($"Property {plExpression.MemberName} is not a TimeSpan");
		}

		if (!Enum.TryParse(plExpression.Operator, out ExpressionType _)) {
			return EngineOperatorResult.NotAValidFor($"{plExpression.Operator} is not a valid ExpressionType");
		}

		if (plExpression.TargetValue.IsSingleValue) {
			if (!TimeSpan.TryParse(plExpression.TargetValue.SingleValue.ToString(), out var ts)) {
				return
						EngineOperatorResult
								.NotAValidFor($"{plExpression.TargetValue.SingleValue} is not a valid TimeSpan");
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
				return EngineOperatorResult.Error(errors.Where(a => a.Kind is not EngineOperatorResultKind.Success)
														.ToArray());
			}
		}

		return EngineOperatorResult.Success();
	}

	/// <inheritdoc />
	public ParsedValueExpressions GetOperator<T>(SmartPlExpression   plExpression,
											 MemberExpression    sourceExpression,
											 ParameterExpression parameterExpression,
											 Type                parameterPropertyType) {

		var tBinary = Enum.Parse<ExpressionType>(plExpression.Operator);

		if (plExpression.TargetValue.IsSingleValue) {
			return new(plExpression.Match,
					   BuildComparisonExpression(plExpression,
												 sourceExpression,
												 tBinary,
												 parameterPropertyType,
												 plExpression.TargetValue.SingleValue));
		}

		return new(plExpression.Match,
				   GetAllExpressions(plExpression,
									 sourceExpression,
									 tBinary,
									 parameterPropertyType));
	}



	private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression plExpression,
																		MemberExpression  sourceExpression,
																		ExpressionType    tBinary,
																		Type              propertyType) {
		foreach (var value in plExpression.TargetValue.GetValues()) {
			yield return BuildComparisonExpression(plExpression, sourceExpression, tBinary, propertyType, value);
		}
	}

	private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression plExpression,
																   MemberExpression  sourceExpression,
																   ExpressionType    tBinary,
																   Type              propertyType,
																   object            value) {
		var parsedValue = TimeSpan.Parse(value.ToString());

		var rightValue = parsedValue.ToConstantExpressionAsType(propertyType);

		// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
		var builtExpression = Expression.MakeBinary(tBinary, sourceExpression, rightValue);

		return new(builtExpression, plExpression, value);
	}
}
