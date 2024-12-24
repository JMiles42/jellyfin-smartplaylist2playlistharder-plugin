namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Containers;

public sealed class CompiledExpression<T> : IReadOnlyCollection<CompiledExpressionResult<T>>
{
    private readonly List<CompiledExpressionResult<T>> _compiledExpressions = [];

    public MatchMode Match { get; init; }

    public IReadOnlyList<CompiledExpressionResult<T>> CompiledExpressions => _compiledExpressions;

    public void Add(CompiledExpressionResult<T> expression) => _compiledExpressions.Add(expression);

    /// <inheritdoc />
    public IEnumerator<CompiledExpressionResult<T>> GetEnumerator() => CompiledExpressions.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count => CompiledExpressions.Count;
}

public sealed class CompiledExpressionSet<T> : IReadOnlyCollection<CompiledExpression<T>>
{
    private readonly List<CompiledExpression<T>> _compiledExpressionSets;

    public MatchMode Match { get; init; }

    public IReadOnlyList<CompiledExpression<T>> CompiledExpressionSets => _compiledExpressionSets;

    public CompiledExpressionSet() => _compiledExpressionSets = [];

    public CompiledExpressionSet(MatchMode match)
    {
        _compiledExpressionSets = [];
        Match = match;
    }

    public CompiledExpressionSet(MatchMode match, IEnumerable<CompiledExpression<T>> values)
    {
        Match = match;
        _compiledExpressionSets = [.. values];
    }

    public void Add(CompiledExpression<T> expression) => _compiledExpressionSets.Add(expression);

    /// <inheritdoc />
    public IEnumerator<CompiledExpression<T>> GetEnumerator() => CompiledExpressionSets.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count => CompiledExpressionSets.Count;
}