using System.Linq.Expressions;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class ExpressionTypeOperator: IOperator
{

	private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression plExpression,
																			  MemberExpression  sourceExpression,
																			  ExpressionType    tBinary,
																			  Type              propertyType)
	{
		foreach (var value in plExpression.TargetValue.GetValues())
		{
			yield return BuildComparisonExpression(plExpression, sourceExpression, tBinary, propertyType, value);
		}
	}

	private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression plExpression,
																		 MemberExpression  sourceExpression,
																		 ExpressionType    tBinary,
																		 Type              propertyType,
																		 object            value)
	{
		var rightValue = value.ToConstantExpressionAsType(propertyType);

		// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
		var builtExpression = Expression.MakeBinary(tBinary, sourceExpression, rightValue);

		return new(builtExpression, plExpression, value);
	}

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType)
	{
		if (!Enum.TryParse(plExpression.Operator, out ExpressionType _))
		{
			return EngineOperatorResult.NotAValidFor($"{plExpression.Operator} is not a valid ExpressionType");
		}

		return EngineOperatorResult.Success();
	}

	/// <inheritdoc />
	public ParsedValueExpressions GetOperator<T>(SmartPlExpression   plExpression,
												 MemberExpression    sourceExpression,
												 ParameterExpression parameterExpression,
												 Type                parameterPropertyType)
	{
		Enum.TryParse(plExpression.Operator, out ExpressionType tBinary);


		if (plExpression.TargetValue.IsSingleValue)
		{
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
}
