using System.Collections;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

public class ParsedValueExpressions : IReadOnlyCollection<ParsedValueExpressionResult>
{
    public MatchMode Match { get; init; }

    public ParsedValueExpressions(MatchMode match)
    {
        Match = match;
        _builtExpressionResult = new();
    }

    public ParsedValueExpressions(MatchMode match, ParsedValueExpressionResult singleResult)
    {
        Match = match;
        _builtExpressionResult = new() { singleResult };
    }

    public ParsedValueExpressions(MatchMode match, IEnumerable<ParsedValueExpressionResult> builtRange)
    {
        Match = match;
        _builtExpressionResult = new(builtRange);
    }

    private readonly List<ParsedValueExpressionResult> _builtExpressionResult;

    public IReadOnlyList<ParsedValueExpressionResult> BuiltExpressionResult => _builtExpressionResult;

    public void Add(ParsedValueExpressionResult expression)
    {
        _builtExpressionResult.Add(expression);
    }

    /// <inheritdoc />
    public IEnumerator<ParsedValueExpressionResult> GetEnumerator() => BuiltExpressionResult.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count => BuiltExpressionResult.Count;
}
