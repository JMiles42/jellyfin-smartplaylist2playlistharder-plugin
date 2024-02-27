using System.Linq.Expressions;
using System.Reflection;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public class RegexOperator : IOperator {
	private static readonly Type[]     StringTypeArray = { typeof(string) };
	private static readonly MethodInfo RegexIsMatch    = typeof(Regex)!.GetMethod(nameof(Regex.IsMatch), StringTypeArray);

	/// <inheritdoc />
	public EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
													MemberExpression    sourceExpression,
													ParameterExpression parameterExpression,
													Type                parameterPropertyType) {
		if (plExpression.OperatorAsLower is not ("regex" or "matchregex")) {
			return EngineOperatorResult.NotAValidFor();
		}

		if (plExpression.TargetValue.IsSingleValue) {
			var pattern = plExpression.TargetValue.SingleValue.ToString();
			if (!IsValidIshRegex(pattern)) {
				return EngineOperatorResult.Error($"Regex \"{pattern}\" is invalid");
			}
		}
		else {
			var rsts = plExpression.TargetValue.GetValues()
						.Cast<string>()
						.Select(pattern => {
							if (!IsValidIshRegex(pattern)) {
								return EngineOperatorResult.Error($"Regex \"{pattern}\" is invalid");
							}

							return EngineOperatorResult.Success($"Regex \"{pattern}\" is valid.");
						}).ToArray();

			if (rsts.Any(a => a.Kind is not EngineOperatorResultKind.Success)) {
				return EngineOperatorResult.Error(rsts.ToArray());
			}
		}

		return EngineOperatorResult.Success();
	}

	private static bool IsValidIshRegex(string pattern) {
		try {
			Regex.IsMatch(string.Empty, pattern);

			return true;
		}
		catch {
			//
		}
		return false;
	}

	/// <inheritdoc />
	public Expression GetOperator<T>(SmartPlExpression   plExpression,
									 MemberExpression    sourceExpression,
									 ParameterExpression parameterExpression,
									 Type                parameterPropertyType) {
		var options = plExpression.StringComparison switch {
				StringComparison.CurrentCulture => RegexOptions.None,
				StringComparison.InvariantCulture => RegexOptions.None,
				StringComparison.Ordinal => RegexOptions.None,
				_ => RegexOptions.IgnoreCase
		};

		if (plExpression.TargetValue.IsSingleValue) {
			return BuildComparisonExpression(plExpression,
											 sourceExpression,
											 options,
											 parameterPropertyType,
											 plExpression.TargetValue);
		}

		return GetAllExpressions(plExpression, sourceExpression, options, parameterPropertyType)
				.CombineExpressions(plExpression.Match);

	}

	private static IEnumerable<Expression> GetAllExpressions(SmartPlExpression smartPlExpression,
															 MemberExpression  sourceExpression,
															 RegexOptions      options,
															 Type              propertyType) {
		foreach (var value in smartPlExpression.TargetValue.GetValues()) {
				yield return BuildComparisonExpression(smartPlExpression,
													   sourceExpression,
													   options,
													   propertyType,
													   value);
		}
	}

	private static Expression BuildComparisonExpression(SmartPlExpression smartPlExpression,
														MemberExpression  sourceExpression,
														RegexOptions      options,
														Type              propertyType,
														object            value) {
		var regex = new Regex(value?.ToString() ?? string.Empty, options);

		var callInstance = regex.ToConstantExpression();

		var toStringMethod = propertyType.GetMethod(nameof(string.ToString), Array.Empty<Type>())!;

		var methodParam = Expression.Call(sourceExpression, toStringMethod);

		return Expression.Call(callInstance, RegexIsMatch, methodParam);
	}
}
