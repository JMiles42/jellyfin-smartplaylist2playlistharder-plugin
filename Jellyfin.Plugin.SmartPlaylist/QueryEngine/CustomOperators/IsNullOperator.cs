using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.CustomOperators;

public class IsNullOperator: IEngineOperator {

	/// <inheritdoc />
	public bool IsOperatorFor<T>(SmartPlExpression expression, ParameterExpression parameterExpression, Type propertyType) => expression.OperatorAsLower is "null" or "isnull";

	/// <inheritdoc />
	public bool GetOperatorFor<T>(SmartPlExpression expression, MemberExpression sourceExpression, ParameterExpression parameterExpression, Type propertyType, out Expression resultExpression) {
		resultExpression = Expression.MakeBinary(ExpressionType.Equal, sourceExpression, Expression.Constant(null));

		return true;
	}
}
