using System.Linq.Expressions;
using System.Reflection;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Expression = Jellyfin.Plugin.SmartPlaylist.Models.Expression;
using linqExpression = System.Linq.Expressions.Expression;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

// This was based off of  https://stackoverflow.com/questions/6488034/how-to-implement-a-rule-engine
// When first written in https://github.com/ankenyr/jellyfin-smartplaylist-plugin which this repo is a fork of
public class Engine {

	private static readonly Type[] StringTypeArray = { typeof(string) };
	private static readonly Type[] StringAndComparisonTypeArray = { typeof(string), typeof(StringComparison) };

	private static readonly DateTime _origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	private static linqExpression BuildExpr<T>(Expression r, ParameterExpression param) {
		var left = linqExpression.Property(param, r.MemberName);

		var tProp = typeof(T).GetProperty(r.MemberName)?.PropertyType;
		ArgumentNullException.ThrowIfNull(tProp);

		if (tProp.Name == "String") {
			return ProcessString(r, tProp, left);
		}

		if (r.Operator is "MatchRegex" or "NotMatchRegex") {
			if (ProcessRegex(r, tProp, left, out var expression)) {
				return expression;
			}
		}

		if (r.Operator is "StringListContainsSubstring") {
			if (ProcessStringListContains(r, tProp, left, out var expression)) {
				return expression;
			}
		}

		// is the operator a known .NET operator?
		if (Enum.TryParse(r.Operator, out ExpressionType tBinary)) {
			return ProcessEnum(r, tProp, tBinary, left);
		}

		return ProcessFallback(r, tProp, left);
	}

	private static linqExpression ProcessEnum(Expression r, Type tProp, ExpressionType tBinary, MemberExpression left) {
		var right = linqExpression.Constant(Convert.ChangeType(r.TargetValue, tProp));

		// use a binary operation, e.g. 'Equal' -> 'u.Age == 15'
		return linqExpression.MakeBinary(tBinary, left, right).InvertIfTrue(r.InvertResult);
	}

	private static linqExpression ProcessFallback(Expression r, Type tProp, MemberExpression left) {
		var method = tProp.GetMethod(r.Operator);

		ArgumentNullException.ThrowIfNull(method);

		var tParam = method.GetParameters()[0].ParameterType;
		var right = linqExpression.Constant(Convert.ChangeType(r.TargetValue, tParam));

		// use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag)'
		return linqExpression.Call(left, method, right).InvertIfTrue(r.InvertResult);
	}

	private static linqExpression ProcessString(Expression r, Type tProp, MemberExpression left) {
		var method = tProp.GetMethod(r.Operator, StringAndComparisonTypeArray);

		ArgumentNullException.ThrowIfNull(method);

		var tParam = method.GetParameters()[0].ParameterType;
		var right = linqExpression.Constant(Convert.ChangeType(r.TargetValue, tParam));
		var stringComparison = linqExpression.Constant(r.StringComparison);

		// use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag, stringComparison)'
		return linqExpression.Call(left, method, right, stringComparison).InvertIfTrue(r.InvertResult);
	}

	private static bool ProcessStringListContains(Expression r,
												  Type tProp,
												  MemberExpression left,
												  out linqExpression expression) {
		expression = null;

		if (r.Operator is "StringListContainsSubstring") {
			if (!tProp.IsAssignableTo(typeof(IReadOnlyCollection<string>))) {
				return false;
			}

			var right = linqExpression.Constant(Convert.ChangeType(r.TargetValue, typeof(string)));
			var stringComparison = linqExpression.Constant(r.StringComparison);

			// use a method call 'u.Tags.Any(a => a.Contains(some_tag))'
			expression = linqExpression.Call(null, EngineExtensions.StringArrayContainsMethodInfo, left, right, stringComparison).InvertIfTrue(r.InvertResult);

			return true;
		}

		return false;
	}

	private static bool ProcessRegex(Expression r, Type tProp, MemberExpression left, out linqExpression expression) {
		var options = r.StringComparison switch {
			StringComparison.CurrentCulture => RegexOptions.None,
			StringComparison.InvariantCulture => RegexOptions.None,
			StringComparison.Ordinal => RegexOptions.None,
			_ => RegexOptions.IgnoreCase
		};

		var regex = new Regex(r.TargetValue, options);
		var method = typeof(Regex).GetMethod("IsMatch", StringTypeArray);
		ArgumentNullException.ThrowIfNull(method);

		Debug.Assert(method != null, nameof(method) + " != null");
		var callInstance = linqExpression.Constant(regex);

		var toStringMethod = tProp.GetMethod("ToString", Array.Empty<Type>());
		Debug.Assert(toStringMethod != null, nameof(toStringMethod) + " != null");
		var methodParam = linqExpression.Call(left, toStringMethod);

		var call = linqExpression.Call(callInstance, method, methodParam);

		switch (r.Operator) {
			case "MatchRegex": {
					expression = call.InvertIfTrue(r.InvertResult);

					return true;
				}
			case "NotMatchRegex": {
					expression = linqExpression.Not(call).InvertIfTrue(r.InvertResult);

					return true;
				}
			default:
				expression = null;

				return false;
		}
	}

	public static Func<T, bool> CompileRule<T>(Expression r) {
		var paramUser = linqExpression.Parameter(typeof(Operand));
		var expr = BuildExpr<T>(r, paramUser);
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
		foreach (var rule in rules.Expressions) {
			if (rule.MemberName == "PremiereDate") {
				var date = DateTime.Parse(rule.TargetValue);
				rule.TargetValue = ConvertToUnixTimestamp(date).ToString();
			}
		}

		return rules;
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
