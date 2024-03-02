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

    private CompiledRule     CompiledRule { get; set; }
    public  SmartPlaylistDto Dto          { get; set; }

    public SmartPlaylist(SmartPlaylistDto dto) {
        Dto            = dto;
        Id             = dto.Id;
        Name           = dto.Name;
        FileName       = dto.FileName;
        User           = dto.User;
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
            CompiledRule.CompiledRuleSets.Add(set.Expressions.Where(a => !a.IsInValid).Select(Engine.CompileRule<Operand>).ToList());
        }
    }

    internal List<List<Func<Operand, bool>>> GetCompiledRules() {
        if (CompiledRule is not null) {
            return CompiledRule.CompiledRuleSets;
        }

        CompileRules();

        return CompiledRule!.CompiledRuleSets;
    }

    // Returns the BaseItems that match the filter, if order is provided the IDs are sorted.
    public IEnumerable<BaseItem> FilterPlaylistItems(IEnumerable<BaseItem> items,
                                                     ILibraryManager       libraryManager,
                                                     User                  user) {
        var sorter = new Sorter(this);

        foreach (var i in items) {
            var opp = new Operand(libraryManager, i, BaseItem.UserDataManager, user);
            sorter.SortItem(opp);
        }

        return sorter.GetResults();
    }

    public Sorter GetSorter() => new(this);

    private static bool ProcessRule(List<Func<Operand, bool>> set, Operand operand) {
        return set.All(rule => rule(operand));
    }

    public class Sorter
    {
        private readonly List<BaseItem>                  Items = new (1000);
        private readonly SmartPlaylist                   _owner;
        private readonly List<List<Func<Operand, bool>>> _rules;

        internal Sorter(SmartPlaylist owner) {
            _owner = owner;
            _rules = _owner.GetCompiledRules();
        }

        public void SortItem(Operand         item) {

            if (_rules.Any(set => ProcessRule(set, item))) {
                Items.Add(item.BaseItem);
            }
        }

        public IEnumerable<BaseItem> GetResults() {
            var enumerable = _owner.Order.OrderItems(Items);

            return enumerable;
        }
    }
}
