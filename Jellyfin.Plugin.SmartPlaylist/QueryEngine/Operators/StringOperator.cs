using System.Linq.Expressions;
using System.Reflection;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class StringOperator: IOperator {
	private static readonly Type[] StringAndComparisonTypeArray = { typeof(string), typeof(StringComparison) };

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType) {
		if (parameterPropertyType != typeof(string)) {
			return EngineOperatorResult.NotAValidFor("Parameter is not a string");
		}

		var method = GetMethod(plExpression, parameterPropertyType);

		if (method is null) {
			return EngineOperatorResult.NotAValidFor($"The operator name {plExpression.Operator} is not a valid method on a string");
		}

		return EngineOperatorResult.Success();
	}

	private static MethodInfo? GetMethod(SmartPlExpression plExpression, Type parameterPropertyType) => parameterPropertyType.GetMethod(plExpression.Operator, StringAndComparisonTypeArray);

	/// <inheritdoc />
	public Expression GetOperator<T>(SmartPlExpression   plExpression,
									 MemberExpression    sourceExpression,
									 ParameterExpression parameterExpression,
									 Type                parameterPropertyType) {
		var method              = GetMethod(plExpression, parameterPropertyType);
		var methodParameterType = method.GetParameters()[0].ParameterType;


		if (plExpression.TargetValue.IsSingleValue) {
			return BuildComparisonExpression(plExpression, sourceExpression, methodParameterType, method, plExpression.TargetValue.SingleValue);
		}

		return GetAllExpressions(plExpression, sourceExpression, methodParameterType, method).CombineExpressions(plExpression.Match);
	}
	private static IEnumerable<Expression> GetAllExpressions(SmartPlExpression expression,
															 MemberExpression  sourceExpression,
															 Type              parameterType,
															 MethodInfo        method) {
		foreach (var value in expression.TargetValue.GetValues()) {
			yield return BuildComparisonExpression(expression, sourceExpression, parameterType, method, value);
		}
	}

	private static Expression BuildComparisonExpression(SmartPlExpression expression,
														MemberExpression  sourceExpression,
														Type              parameterType,
														MethodInfo        method,
														object            value) {
		var rightValue = value.ToConstantExpressionAsType(parameterType);
		var stringComparison = Expression.Constant(expression.StringComparison);

		// use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag, stringComparison)'
		return Expression.Call(sourceExpression, method, rightValue, stringComparison);
	}
}
