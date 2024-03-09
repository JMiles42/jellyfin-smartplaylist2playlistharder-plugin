namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.RuleFixers;

public static class ExpressionFixerManager
{

    private static readonly IReadOnlyDictionary<string, IExpressionFixer> Fixers =
            new Dictionary<string, IExpressionFixer>
            {
                { nameof(Operand.PremiereDate), new PremiereDateExpressionFixer() },
            };

    public static void FixRules(List<SmartPlExpression> rulesExpressions)
    {
        foreach (var rule in rulesExpressions.Where(rule => Fixers.ContainsKey(rule.MemberName)))
        {
            Fixers[rule.MemberName].FixExpression(rule);
        }
    }
}
