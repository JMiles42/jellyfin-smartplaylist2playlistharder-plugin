using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.CustomOperators;

public class StringListContainsSubstringOperator : IEngineOperator {
	/// <inheritdoc />
	public bool IsOperatorFor<T>(SmartPlExpression   expression,
								 ParameterExpression parameterExpression,
								 Type                propertyType) =>
			expression.OperatorAsLower is "stringlistcontainssubstring";

	/// <inheritdoc />
	public bool GetOperatorFor<T>(SmartPlExpression   expression,
								  MemberExpression    sourceExpression,
								  ParameterExpression parameterExpression,
								  Type                propertyType,
								  out Expression      resultExpression) {

		if (!propertyType.IsAssignableTo(typeof(IReadOnlyCollection<string>))) {
			resultExpression = null;
			return false;
		}

		var right            = Expression.Constant(Convert.ChangeType(expression.TargetValue, typeof(string)));
		var stringComparison = Expression.Constant(expression.StringComparison);

		// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
		resultExpression = Expression
				.Call(null, EngineExtensions.StringArrayContainsMethodInfo, sourceExpression, right, stringComparison);


		return true;
	}
}
