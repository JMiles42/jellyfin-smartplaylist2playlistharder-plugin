namespace Jellyfin.Plugin.SmartPlaylist.Models;

public sealed class SmartPlaylist
{
    public Guid? Id { get; set; }

    public string? Name { get; set; }

    public string? FileName { get; set; }

    public string? User { get; set; }

    public IReadOnlyList<ExpressionSet> ExpressionSets { get; set; }

    public int MaxItems { get; set; }
    public int SkipItems { get; set; }

    public bool IsReadonly { get; set; }

    public MatchMode Match { get; set; }

    public OrderStack Order { get; set; }

    public BaseItemKind[] SupportedItems { get; set; }

    public CompiledPlaylistExpressionSets? CompiledPlaylistExpressionSets { get; set; }

    public SmartPlaylistDto Dto { get; set; }

    public SmartPlaylist(SmartPlaylistDto dto)
    {
        Dto = dto;
        Id = dto.Id;
        Name = dto.Name;
        FileName = dto.FileName;
        User = dto.User;
        MaxItems = dto.MaxItems;
        SupportedItems = dto.SupportedItems;
        IsReadonly = dto.IsReadonly;
        Match = dto.Match;

        Order = GenerateOrderStack(dto.Order);
        ExpressionSets = RulesCompiler.FixRuleSets(dto.ExpressionSets);
    }

    private static OrderStack GenerateOrderStack(OrderByDto? dtoOrder)
    {
        if (dtoOrder is null)
        {
            return OrderManager.Default;
        }

        return new(dtoOrder
            .Where(a => !a.IsInValid)
            .Select(OrderManager.GetOrder)
            .ToArray());
    }

    internal CompiledPlaylistExpressionSets CompilePlaylistExpressionSets()
    {
        CompiledPlaylistExpressionSets compiledPlaylistExpressionSets = new();

        foreach (var set in ExpressionSets)
        {
            var listOfCompiledExpressions = set.Expressions
                .Where(a => a.IsValid)
                .Select(RulesCompiler.CompileRule<Operand>)
                .ToList();

            compiledPlaylistExpressionSets.CompiledExpressionSets.Add(new(set.Match, listOfCompiledExpressions));
        }

        return compiledPlaylistExpressionSets;
    }

    internal List<CompiledExpressionSet<Operand>> GetCompiledRules()
    {
        if (CompiledPlaylistExpressionSets is not null)
        {
            return CompiledPlaylistExpressionSets.CompiledExpressionSets;
        }

        CompiledPlaylistExpressionSets = CompilePlaylistExpressionSets();

        return CompiledPlaylistExpressionSets.CompiledExpressionSets;
    }

    public Sorter GetSorter() => new(this);
}