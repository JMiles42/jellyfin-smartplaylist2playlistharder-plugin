using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Extensions;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public static class OrderManager {
    private static readonly Dictionary<string, OrderPair> _orderPairs = new();
    public static           OrderStack                    Default;
    static OrderManager() {
        RegisterOrders();
        Default = new(NoOrder.Instance);
    }

    private static void RegisterOrders() {
        RegisterOrder(NoOrder.Instance,               NoOrder.Instance);
        RegisterOrder(RandomOrder.Instance,           RandomOrder.Instance);
        RegisterOrder(item => item.Name);
        RegisterOrder(item => item.CommunityRating);
        RegisterOrder(item => item.CriticRating);
        RegisterOrder(item => item.OriginalTitle);
        RegisterOrder(item => item.Path);
        RegisterOrder(item => item.Container);
        RegisterOrder(item => item.Tagline);
        RegisterOrder(item => item.ChannelId);
        RegisterOrder(item => item.Id);
        RegisterOrder(item => item.Width);
        RegisterOrder(item => item.Height);
        RegisterOrder(item => item.PremiereDate,   "ReleaseDate", "Release Date");
        RegisterOrder(item => item.DateModified);
        RegisterOrder(item => item.DateLastSaved);
        RegisterOrder(item => item.DateLastRefreshed);
        RegisterOrder(item => item.DaysSincePremiereDate);
        RegisterOrder(item => item.DateSinceCreated);
        RegisterOrder(item => item.DateSinceModified);
        RegisterOrder(item => item.DateSinceLastSaved);
        RegisterOrder(item => item.DateSinceLastRefreshed);
        RegisterOrder(item => item.MediaType);
        RegisterOrder(item => item.SortName);
        RegisterOrder(item => item.ForcedSortName);
        RegisterOrder(item => item.EndDate);
        RegisterOrder(item => item.Overview);
        RegisterOrder(item => item.ProductionYear, "Year");
        RegisterOrder(item => item.RunTimeTicks);
        RegisterOrder(item => item.CollectionName, "BoxSet");
        RegisterOrder(item => item.HasSubtitles);
        RegisterOrder(item => item.AiredSeasonNumber, "AiredSeasonNumber");
        RegisterOrder(item => item.ParentIndexNumber,  "ParentIndexNumber", "ParentIndex");
        RegisterOrder(item => item.SeasonName,  "Season");
        RegisterOrder(item => item.SeriesName,   "Series");
    }

    private static void RegisterOrder<TKey>(Expression<Func<SortableBaseItem, TKey>> keySelector) => RegisterOrder(keySelector, Array.Empty<string>());

    private static void RegisterOrder<TKey>(Expression<Func<SortableBaseItem, TKey>> keySelector, params string[] orderIds) {
        var      hasMemberName = keySelector.TryGetMemberName(out var memberName);
        string[] ids;

        if (hasMemberName) {
            ids = new string[orderIds.Length+1];
            orderIds.CopyTo(ids, 1);
            ids[0] = memberName;
        }
        else {
            ids = orderIds;
        }

        var compiled   = keySelector.Compile();
        var ascending  = new PropertyOrder<TKey>(compiled, true,  ids);
        var descending = new PropertyOrder<TKey>(compiled, false, ids);

        var pair = new OrderPair(ascending, descending);

        if (hasMemberName) {
            RegisterOrder(memberName, pair);
        }

        foreach (var name in ids) {
            RegisterOrder(name, pair);
        }
    }

    private static void RegisterOrder(string name, OrderPair pair) {
        _orderPairs[name] = pair;
    }

    private static void RegisterOrder<T>(T ascending, T descending) where T : Order {
        foreach (var name in ascending.Names()) {
            _orderPairs[name] = new(ascending, descending);
        }
    }

    public static Order GetOrder(IOrderDetails dto)                  => _orderPairs[dto.Name].Get(dto.Ascending);
    public static Order GetOrder(string name, bool ascending = true) => _orderPairs[name].Get(ascending);

    private class OrderPair {
        private Order Ascending { get; }

        private Order Descending { get; }

        public OrderPair(Order ascending, Order descending) {
            Ascending = ascending;
            Descending = descending;
        }

        public Order Get(bool ascending) => ascending ? Ascending : Descending;
    }
}
