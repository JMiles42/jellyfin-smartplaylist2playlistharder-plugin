using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.RuleFixers;
using linqExpression = System.Linq.Expressions.Expression;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

// This was based off of  https://stackoverflow.com/questions/6488034/how-to-implement-a-rule-engine
// When first written in https://github.com/ankenyr/jellyfin-smartplaylist-plugin which this repo is a fork of
public class Engine
{

	private static readonly OperatorManager OperatorManager = new ();

	private static linqExpression BuildExpr<T>(SmartPlExpression expression, ParameterExpression param) {
		var linqExpr = GetExpression<T>(expression, param);

		return linqExpr.InvertIfTrue(expression.InvertResult);
	}

	private static linqExpression GetExpression<T>(SmartPlExpression r, ParameterExpression param) {
		var left = linqExpression.Property(param, r.MemberName.ToStringFast());

		var tProp = typeof(T).GetProperty(r.MemberName.ToStringFast())?.PropertyType;
		ArgumentNullException.ThrowIfNull(tProp);


		foreach (var engineOperator in OperatorManager.EngineOperators) {
			var opper = engineOperator.ValidateOperator<T>(r, left, param, tProp);

			if (opper.Kind is EngineOperatorResultKind.Success) {
				return engineOperator.GetOperator<T>(r, left, param, tProp);
			}
		}

		return EngineFallbackProcessor.ProcessFallback(r, tProp, left);
	}

	public static Func<T, bool> CompileRule<T>(SmartPlExpression r) {
		if (r.IsInValid) {
			return t => false;
		}

		var paramUser = linqExpression.Parameter(typeof(Operand));
		var expr      = BuildExpr<T>(r, paramUser);

		// build a lambda function User->bool and compile it
		var value = linqExpression.Lambda<Func<T, bool>>(expr, paramUser).Compile(true);

		return value;
	}

	public static List<ExpressionSet>? FixRuleSets(List<ExpressionSet>? ruleSets) {
		if (ruleSets is null) {
			return null;
		}

		foreach (var rules in ruleSets) {
			FixRules(rules);
		}

		return ruleSets;
	}

	public static ExpressionSet FixRules(ExpressionSet rules) {
		ExpressionFixerManager.FixRules(rules.Expressions);

		return rules;
	}
}