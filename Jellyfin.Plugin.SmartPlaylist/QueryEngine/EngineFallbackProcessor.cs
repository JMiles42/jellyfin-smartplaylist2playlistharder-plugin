using System.Linq.Expressions;
using System.Reflection;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

public static class EngineFallbackProcessor
{
	public static Expression ProcessFallback(SmartPlExpression plExpression, Type tProp, MemberExpression left) {
		var method = tProp.GetMethod(plExpression.Operator);

		if (method is null) {
			throw new InvalidOperationException($"Operator '{plExpression.Operator}' is not a valid method to call on MemberName '{plExpression.MemberName}' of type: {tProp}");
		}

		if (plExpression.TargetValue.IsSingleValue) {
			return BuildComparisonExpression(plExpression,
											 left,
											 method,
											 plExpression.TargetValue.SingleValue);
		}

		return GetAllExpressions(plExpression,
								 left,
								 method)
				.CombineExpressions(plExpression.Match);
	}

	private static IEnumerable<Expression> GetAllExpressions(SmartPlExpression plExpression, MemberExpression left, MethodInfo method) {
		foreach (var value in plExpression.TargetValue.GetValues()) {
			yield return BuildComparisonExpression(plExpression, left, method, value);
		}
	}

	private static Expression BuildComparisonExpression(SmartPlExpression plExpression,
														MemberExpression  leftValue,
														MethodInfo        method,
														object            value) {
		var tParam = method.GetParameters()[0].ParameterType;
		var rightValue  = value.ToConstantExpressionAsType(tParam);

		// use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag)'
		return Expression.Call(leftValue, method, rightValue);
	}
}
