using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.CustomOperators;
using linqExpression = System.Linq.Expressions.Expression;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

// This was based off of  https://stackoverflow.com/questions/6488034/how-to-implement-a-rule-engine
// When first written in https://github.com/ankenyr/jellyfin-smartplaylist-plugin which this repo is a fork of
public class Engine
{
	private static readonly Type[] StringTypeArray              = { typeof(string) };
	private static readonly Type[] StringAndComparisonTypeArray = { typeof(string), typeof(StringComparison) };

	private static readonly DateTime _origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);


	private static readonly IEngineOperator[] engineOperators = {
			new StringOperator(),
			new IsNullOperator(),
			new RegexOperator(),
			new StringListContainsSubstringOperator(),
			new ExpressionTypeOperator(),
	};

	private static linqExpression BuildExpr<T>(SmartPlExpression expression, ParameterExpression param) {
		var linqExpr = GetExpression<T>(expression, param);

		return linqExpr.InvertIfTrue(expression.InvertResult);
	}

	private static linqExpression GetExpression<T>(SmartPlExpression r, ParameterExpression param) {
		var left = linqExpression.Property(param, r.MemberName);

		var tProp = typeof(T).GetProperty(r.MemberName)?.PropertyType;
		ArgumentNullException.ThrowIfNull(tProp);


		foreach (var engineOperator in engineOperators) {
			if (!engineOperator.IsOperatorFor<T>(r, param, tProp)) {
				continue;
			}

			if (engineOperator.GetOperatorFor<T>(r, left, param, tProp, out var resultExpression)) {
				return resultExpression;
			}
		}

		return ProcessFallback(r, tProp, left);
	}

	private static linqExpression ProcessFallback(SmartPlExpression r, Type tProp, MemberExpression left) {
		var method = tProp.GetMethod(r.Operator);

		ArgumentNullException.ThrowIfNull(method);

		var tParam = method.GetParameters()[0].ParameterType;
		var right  = linqExpression.Constant(Convert.ChangeType(r.TargetValue, tParam));

		// use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag)'
		return linqExpression.Call(left, method, right);
	}

	public static Func<T, bool> CompileRule<T>(SmartPlExpression r) {
		var paramUser = linqExpression.Parameter(typeof(Operand));
		var expr      = BuildExpr<T>(r, paramUser);
		// build a lambda function User->bool and compile it
		var value = linqExpression.Lambda<Func<T, bool>>(expr, paramUser).Compile(true);

		return value;
	}

	public static List<ExpressionSet> FixRuleSets(List<ExpressionSet> ruleSets) {
		foreach (var rules in ruleSets) {
			FixRules(rules);
		}

		return ruleSets;
	}

	public static ExpressionSet FixRules(ExpressionSet rules) {
		FixRulesImplementation(rules.Expressions);

		return rules;
	}

	private static void FixRulesImplementation(IEnumerable<SmartPlExpression> set) {
		foreach (var rule in set) {
			if (rule.MemberName == "PremiereDate") {
				var date = DateTime.Parse(rule.TargetValue);
				rule.TargetValue = ConvertToUnixTimestamp(date).ToString(CultureInfo.InvariantCulture);
			}
		}
	}

	public static double ConvertToUnixTimestamp(DateTime date) {
		var diff = date.ToUniversalTime() - _origin;

		return Math.Floor(diff.TotalSeconds);
	}
}

internal static class EngineExtensions {

	public static readonly MethodInfo StringArrayContainsMethodInfo =
			typeof(EngineExtensions).GetMethod(nameof(StringArrayContains), BindingFlags.Static | BindingFlags.Public);

	public static bool StringArrayContains(this IReadOnlyCollection<string> l, string r, StringComparison stringComparison) {
		return l.Any(a => a.Contains(r, stringComparison));
	}

	public static linqExpression InvertIfTrue(this linqExpression expression, bool invert) =>
			invert ? linqExpression.Not(expression) : expression;
}
