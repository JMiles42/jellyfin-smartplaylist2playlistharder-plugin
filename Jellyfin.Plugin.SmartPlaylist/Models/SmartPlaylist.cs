using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class SmartPlaylist {
    public string Id { get; set; }

    public string Name { get; set; }

    public string FileName { get; set; }

    public string User { get; set; }

    public IReadOnlyList<ExpressionSet> ExpressionSets { get; set; }

    public int MaxItems { get; set; }

    public bool IsReadonly { get; set; }

    public OrderStack Order { get; set; }

    public BaseItemKind[] SupportedItems { get; set; }

    private CompiledRule CompiledRule { get; set; }

    public SmartPlaylist(SmartPlaylistDto dto) {
        Id = dto.Id;
        Name = dto.Name;
        FileName = dto.FileName;
        User = dto.User;
        ExpressionSets = Engine.FixRuleSets(dto.ExpressionSets);

        if (dto.MaxItems > 0) {
            MaxItems = dto.MaxItems;
        }
        else {
            MaxItems = 0;
        }

        Order = GenerateOrderStack(dto.Order);
        SupportedItems = dto.SupportedItems;
    }

    private static OrderStack GenerateOrderStack(OrderByDto dtoOrder) {
        var result = new List<Order>(1 + (dtoOrder.ThenBy?.Count ?? 0)) {
                OrderManager.GetOrder(dtoOrder)
        };

        if (dtoOrder.ThenBy?.Count > 0) {
            foreach (var order in dtoOrder.ThenBy) {
                result.Add(OrderManager.GetOrder(order));
            }
        }

        return new(result.ToArray());
    }

    internal void CompileRules() {
        CompiledRule = new ();

        foreach (var set in ExpressionSets) {
            CompiledRule.CompiledRuleSets.Add(set.Expressions.Select(Engine.CompileRule<Operand>).ToList());
        }
    }

    internal List<List<Func<Operand, bool>>> GetCompiledRules() {
        if (CompiledRule is not null)
            return CompiledRule.CompiledRuleSets;

        CompileRules();

        return CompiledRule!.CompiledRuleSets;
    }

    // Returns the BaseItems that match the filter, if order is provided the IDs are sorted.
    public IEnumerable<Guid> FilterPlaylistItems(IEnumerable<BaseItem> items,
                                                     ILibraryManager libraryManager,
                                                     User user) {
        var results = new List<BaseItem>();

        var compiledRules = GetCompiledRules();
        foreach (var i in items) {
            var operand = OperandFactory.GetMediaType(libraryManager, i, user);

            if (compiledRules.Any(set=>ProcessRule(set, operand))) {
                results.Add(i);
            }
        }

        var enumerable = Order.OrderItems(results);

        return enumerable.Select(bi => bi.Id);
    }

    private static bool ProcessRule(List<Func<Operand, bool>> set, Operand operand) {
            return set.All(rule => rule(operand));
    }
}
