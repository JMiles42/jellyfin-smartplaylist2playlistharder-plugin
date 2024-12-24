namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Model;

internal sealed class OrderPair
{
    private Order Ascending { get; }

    private Order Descending { get; }

    public OrderPair(Order ascending, Order descending)
    {
        Ascending = ascending;
        Descending = descending;
    }

    public Order Get(bool ascending = true) => ascending ? Ascending : Descending;
}