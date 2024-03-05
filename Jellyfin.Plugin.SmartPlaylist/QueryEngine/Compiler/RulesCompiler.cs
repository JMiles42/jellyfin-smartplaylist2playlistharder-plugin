namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Compiler;

// This was based off of  https://stackoverflow.com/questions/6488034/how-to-implement-a-rule-engine
// When first written in https://github.com/ankenyr/jellyfin-smartplaylist-plugin which this repo is a fork of
public static class RulesCompiler
{
	private static readonly OperatorManager OperatorManager = new();

	private static ParsedValueExpressions BuildExpr<T>(SmartPlExpression   expression,
													   ParameterExpression param) =>
			GetExpression<T>(expression, param);

	private static ParsedValueExpressions GetExpression<T>(SmartPlExpression r, ParameterExpression param)
	{
		var left = LinqExpression.Property(param, r.MemberName.ToStringFast());

		var tProp = typeof(T).GetProperty(r.MemberName.ToStringFast())?.PropertyType;
		ArgumentNullException.ThrowIfNull(tProp);


		foreach (var engineOperator in OperatorManager.EngineOperators)
		{
			var engineOperatorResult = engineOperator.ValidateOperator<T>(r, left, param, tProp);

			if (engineOperatorResult.Kind is not EngineOperatorResultKind.Success)
			{
				continue;
			}

			return engineOperator.GetOperator<T>(r, left, param, tProp);
		}

		return EngineFallbackProcessor.ProcessFallback(r, tProp, left);
	}

	private static bool JunkMethod<T>(T t) => false;

	public static CompiledExpression<T> CompileRule<T>(SmartPlExpression plExpression)
	{
		CompiledExpression<T> results = new() { Match = plExpression.Match, };

		if (plExpression.IsInValid)
		{
			results.Add(new(JunkMethod, ParsedValueExpressionResult.Empty));

			return results;
		}

		var sourceTypeParameter = LinqExpression.Parameter(typeof(Operand));
		var builtExpressionSet  = BuildExpr<T>(plExpression, sourceTypeParameter);

		foreach (var builtExpressionResult in builtExpressionSet)
		{
			Func<T, bool> compiledValueChecker = JunkMethod;
			Exception?    error                = null;

			try
			{
				compiledValueChecker = LinqExpression.Lambda<Func<T, bool>>(builtExpressionResult.Expression,
																			sourceTypeParameter)
													 .Compile();
			}
			catch (Exception ex)
			{
				error = ex;
			}

			results.Add(new(compiledValueChecker, builtExpressionResult, error));
		}

		return results;
	}

	public static List<ExpressionSet>? FixRuleSets(List<ExpressionSet>? ruleSets)
	{
		if (ruleSets is null)
		{
			return null;
		}

		foreach (var rules in ruleSets)
		{
			FixRules(rules);
		}

		return ruleSets;
	}

	public static ExpressionSet FixRules(ExpressionSet rules)
	{
		ExpressionFixerManager.FixRules(rules.Expressions);

		return rules;
	}
}
