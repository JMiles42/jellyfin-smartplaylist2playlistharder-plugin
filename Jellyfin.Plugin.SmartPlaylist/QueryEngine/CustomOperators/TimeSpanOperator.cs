using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.CustomOperators;

public class TimeSpanOperator : IEngineOperator {
	/// <inheritdoc />
	public bool IsOperatorFor<T>(SmartPlExpression expression, ParameterExpression parameterExpression, Type propertyType) => propertyType.Name == "TimeSpan";

	/// <inheritdoc />
	public bool GetOperatorFor<T>(SmartPlExpression   expression,
								  MemberExpression    sourceExpression,
								  ParameterExpression parameterExpression,
								  Type                propertyType,
								  out Expression      resultExpression) {
		if (!Enum.TryParse(expression.Operator, out ExpressionType tBinary)) {
			resultExpression = null;
			return false;
		}

		if (!TimeSpan.TryParse(expression.TargetValue, out var ts)) {
			resultExpression = null;
			return false;
		}

		var right = Expression.Constant(ts);

		// use a binary operation, e.g. 'Equal' -> 'u.Age == 15'
		resultExpression = Expression.MakeBinary(tBinary, sourceExpression, right);

		return true;
	}
}
