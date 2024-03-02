using System.Collections;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

public class CompiledExpression<T> : IReadOnlyCollection<CompiledExpressionResult<T>>
{
    private readonly List<CompiledExpressionResult<T>> _compiledExpressions = new();

    public MatchMode Match { get; init; }

    public IReadOnlyList<CompiledExpressionResult<T>> CompiledExpressions => _compiledExpressions;

    public void Add(CompiledExpressionResult<T> expression)
    {
        _compiledExpressions.Add(expression);
    }

    /// <inheritdoc />
    public IEnumerator<CompiledExpressionResult<T>> GetEnumerator() => CompiledExpressions.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count => CompiledExpressions.Count;
}

public class CompiledExpressionSet<T> : IReadOnlyCollection<CompiledExpression<T>>
{
    public CompiledExpressionSet()
    {
        _compiledExpressionSets = new();
    }

    public CompiledExpressionSet(MatchMode match)
    {
        _compiledExpressionSets = new();
        Match = match;
    }

    public CompiledExpressionSet(MatchMode match, IEnumerable<CompiledExpression<T>> values)
    {
        Match = match;
        _compiledExpressionSets = new(values);
    }

    private readonly List<CompiledExpression<T>> _compiledExpressionSets;

    public MatchMode Match { get; init; }

    public IReadOnlyList<CompiledExpression<T>> CompiledExpressionSets => _compiledExpressionSets;

    public void Add(CompiledExpression<T> expression)
    {
        _compiledExpressionSets.Add(expression);
    }

    /// <inheritdoc />
    public IEnumerator<CompiledExpression<T>> GetEnumerator() => CompiledExpressionSets.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count => CompiledExpressionSets.Count;
}
