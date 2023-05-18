using System.Linq.Expressions;
using System.Reflection;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.CustomOperators;

public class RegexOperator : IEngineOperator {
	private static readonly Type[]     StringTypeArray = { typeof(string) };
	private static readonly MethodInfo RegexIsMatch    = typeof(Regex).GetMethod("IsMatch", StringTypeArray);
	/// <inheritdoc />
	public bool IsOperatorFor<T>(SmartPlExpression   smartPlExpression,
								 ParameterExpression parameterExpression,
								 Type                propertyType) =>
			smartPlExpression.OperatorAsLower is "regex" or "matchregex";

	/// <inheritdoc />
	public bool GetOperatorFor<T>(SmartPlExpression                             smartPlExpression,
								  MemberExpression                       sourceExpression,
								  ParameterExpression                    parameterExpression,
								  Type                                   propertyType,
								  out Expression resultExpression) {
		var options = smartPlExpression.StringComparison switch {
				StringComparison.CurrentCulture => RegexOptions.None,
				StringComparison.InvariantCulture => RegexOptions.None,
				StringComparison.Ordinal => RegexOptions.None,
				_ => RegexOptions.IgnoreCase
		};

		var regex  = new Regex(smartPlExpression.TargetValue, options);

		var callInstance = Expression.Constant(regex);

		var toStringMethod = propertyType.GetMethod("ToString", Array.Empty<Type>());

		if (toStringMethod is null) {
			resultExpression = null;
			return false;
		}

		var methodParam = Expression.Call(sourceExpression, toStringMethod);

		var call = Expression.Call(callInstance, RegexIsMatch, methodParam);

		resultExpression = call;
		return true;
	}
}
