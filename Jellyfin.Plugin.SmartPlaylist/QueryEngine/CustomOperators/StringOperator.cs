using Jellyfin.Plugin.SmartPlaylist.Models;
using System.Linq.Expressions;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.CustomOperators;

public class StringOperator: IEngineOperator {
	private static readonly Type[] StringAndComparisonTypeArray = { typeof(string), typeof(StringComparison) };

	/// <inheritdoc />
	public bool IsOperatorFor<T>(SmartPlExpression expression, ParameterExpression parameterExpression, Type propertyType) => propertyType.Name == "String";

	/// <inheritdoc />
	public bool GetOperatorFor<T>(SmartPlExpression   expression,
								  MemberExpression    sourceExpression,
								  ParameterExpression parameterExpression,
								  Type                propertyType,
								  out Expression  resultExpression) {
		var method = propertyType.GetMethod(expression.Operator, StringAndComparisonTypeArray);

		if (method is null) {
			resultExpression = null;
			return false;
		}

		var tParam           = method.GetParameters()[0].ParameterType;
		var right            = Expression.Constant(Convert.ChangeType(expression.TargetValue, tParam));
		var stringComparison = Expression.Constant(expression.StringComparison);

		// use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag, stringComparison)'
		resultExpression = Expression.Call(sourceExpression, method, right, stringComparison);


		return true;
	}
}
