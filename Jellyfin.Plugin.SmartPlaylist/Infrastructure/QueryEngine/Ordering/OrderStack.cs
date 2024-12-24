namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Ordering;

public sealed class OrderStack
{
    public Order[] Order { get; }

    public OrderStack(params Order[] order) => Order = order;

    public IEnumerable<BaseItem> OrderItems(IEnumerable<Operand> items)
    {
        if (Order.Length == 0)
        {
            return items.Select(a => a.BaseItem);
        }

        var sortableBaseItems = items;

        if (Order.Length == 1)
        {
            return Order[0]
                   .OrderBy(sortableBaseItems)
                   .Select(a => a.BaseItem);
        }

        return OrderMany(sortableBaseItems);
    }

    private IEnumerable<BaseItem> OrderMany(IEnumerable<Operand> items)
    {
        var firstOrder = Order.First();

        var ordered = firstOrder.OrderBy(items);

        foreach (var order in Order.Skip(1))
        {
            ordered = order.ThenBy(ordered);
        }

        return ordered.Select(a => a.BaseItem);
    }
}