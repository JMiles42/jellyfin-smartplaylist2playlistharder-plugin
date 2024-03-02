using System.Linq.Expressions;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class StringOperator: IOperator
{
	private static readonly Type[] StringAndComparisonTypeArray = { typeof(string), typeof(StringComparison) };

	private static MethodInfo? GetMethod(SmartPlExpression plExpression, Type parameterPropertyType) =>
			parameterPropertyType.GetMethod(plExpression.Operator, StringAndComparisonTypeArray);

	private static IEnumerable<ParsedValueExpressionResult> GetAllExpressions(SmartPlExpression plExpression,
																			  MemberExpression  sourceExpression,
																			  Type              parameterType,
																			  MethodInfo        method)
	{
		foreach (var value in plExpression.TargetValue.GetValues())
		{
			yield return BuildComparisonExpression(plExpression, sourceExpression, parameterType, method, value);
		}
	}

	private static ParsedValueExpressionResult BuildComparisonExpression(SmartPlExpression plExpression,
																		 MemberExpression  sourceExpression,
																		 Type              parameterType,
																		 MethodInfo        method,
																		 object            value)
	{
		var rightValue       = value.ToConstantExpressionAsType(parameterType);
		var stringComparison = Expression.Constant(plExpression.StringComparison);

		// use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag, stringComparison)'
		var builtExpression = Expression.Call(sourceExpression, method, rightValue, stringComparison);

		return new(builtExpression, plExpression, value);
	}

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType)
	{
		if (parameterPropertyType != typeof(string))
		{
			return EngineOperatorResult.NotAValidFor("Parameter is not a string");
		}

		var method = GetMethod(plExpression, parameterPropertyType);

		if (method is null)
		{
			return
					EngineOperatorResult
							.NotAValidFor($"The operator name {plExpression.Operator} is not a valid method on a string");
		}

		return EngineOperatorResult.Success();
	}

	/// <inheritdoc />
	public ParsedValueExpressions GetOperator<T>(SmartPlExpression   plExpression,
												 MemberExpression    sourceExpression,
												 ParameterExpression parameterExpression,
												 Type                parameterPropertyType)
	{
		var method              = GetMethod(plExpression, parameterPropertyType);
		var methodParameterType = method.GetParameters()[0].ParameterType;


		if (plExpression.TargetValue.IsSingleValue)
		{
			return new(plExpression.Match,
					   BuildComparisonExpression(plExpression,
												 sourceExpression,
												 methodParameterType,
												 method,
												 plExpression.TargetValue.SingleValue));
		}

		return new(plExpression.Match,
				   GetAllExpressions(plExpression, sourceExpression, methodParameterType, method));
	}
}
