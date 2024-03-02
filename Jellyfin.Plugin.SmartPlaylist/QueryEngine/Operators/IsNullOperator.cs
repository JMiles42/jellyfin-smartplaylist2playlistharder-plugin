using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class IsNullOperator : IOperator
{
	private static readonly ConstantExpression NullExpression = Expression.Constant(null);
	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType) {
		if (plExpression.OperatorAsLower is ("null" or "isnull")) {
			return EngineOperatorResult.Success();
		}

		return EngineOperatorResult.NotAValidFor();
	}

	/// <inheritdoc />
	public ParsedValueExpressions GetOperator<T>(SmartPlExpression   plExpression,
											 MemberExpression    sourceExpression,
											 ParameterExpression parameterExpression,
											 Type                parameterPropertyType) {
		var builtExpression = Expression.MakeBinary(ExpressionType.Equal, sourceExpression, NullExpression);


		return new(plExpression.Match, new ParsedValueExpressionResult(builtExpression, plExpression, null!));
	}
}
