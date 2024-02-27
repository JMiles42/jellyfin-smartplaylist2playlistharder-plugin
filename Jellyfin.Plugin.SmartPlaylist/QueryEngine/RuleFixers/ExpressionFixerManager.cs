using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.RuleFixers;

public static class ExpressionFixerManager
{

	static IReadOnlyDictionary<OperandMember, IExpressionFixer> Fixers = new Dictionary<OperandMember, IExpressionFixer> {
			{ OperandMember.PremiereDate, new PremiereDateExpressionFixer() },
	};

	public static void FixRules(List<SmartPlExpression> rulesExpressions) {
		foreach (var rule in rulesExpressions.Where(rule => Fixers.ContainsKey(rule.MemberName))) {
			Fixers[rule.MemberName].FixExpression(rule);
		}
	}
}