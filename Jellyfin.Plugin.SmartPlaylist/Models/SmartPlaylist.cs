using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class SmartPlaylist {
    public Guid? Id { get; set; }

    public string? Name { get; set; }

    public string? FileName { get; set; }

    public string? User { get; set; }

    public IReadOnlyList<ExpressionSet> ExpressionSets { get; set; }

    public int MaxItems { get; set; }

    public bool IsReadonly { get; set; }

    public OrderStack Order { get; set; }

    public BaseItemKind[] SupportedItems { get; set; }

    public CompiledRule?    CompiledRule { get; set; }
    public SmartPlaylistDto Dto          { get; set; }

    public SmartPlaylist(SmartPlaylistDto dto) {
        Dto            = dto;
        Id             = dto.Id;
        Name           = dto.Name;
        FileName       = dto.FileName;
        User           = dto.User;
        ExpressionSets = Engine.FixRuleSets(dto.ExpressionSets);

        MaxItems = dto.MaxItems;

        Order = GenerateOrderStack(dto.Order);
        SupportedItems = dto.SupportedItems;
    }

    private static OrderStack GenerateOrderStack(OrderByDto? dtoOrder) {
        if (dtoOrder is null) {
            return OrderManager.Default;
        }

        return new(dtoOrder.Select(OrderManager.GetOrder).ToArray());
    }

    internal CompiledRule CompileRules() {
        CompiledRule compiledRule = new ();

        foreach (var set in ExpressionSets) {
            compiledRule.CompiledRuleSets.Add(set.Expressions.Where(a => !a.IsInValid)
                                                 .Select(Engine.CompileRule<Operand>).ToList());
        }

        return compiledRule;
    }

    internal List<List<Func<Operand, bool>>> GetCompiledRules() {
        if (CompiledRule is not null) {
            return CompiledRule.CompiledRuleSets;
        }

        CompiledRule = CompileRules();

        return CompiledRule!.CompiledRuleSets;
    }

    public Sorter GetSorter() => new(this);
}