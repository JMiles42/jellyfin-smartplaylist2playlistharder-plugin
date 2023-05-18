using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.CustomOperators;

public class ExpressionTypeOperator: IEngineOperator
{
	/// <inheritdoc />
	public bool IsOperatorFor<T>(SmartPlExpression   expression,
								 ParameterExpression parameterExpression,
								 Type                propertyType) =>
			Enum.TryParse(expression.Operator, out ExpressionType _);

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

		var right = Expression.Constant(Convert.ChangeType(expression.TargetValue, propertyType));

		// use a binary operation, e.g. 'Equal' -> 'u.Age == 15'
		resultExpression = Expression.MakeBinary(tBinary, sourceExpression, right);

		return true;
	}
}