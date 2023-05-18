using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public static class OrderManager {
    private static readonly Dictionary<string, OrderPair> _orderPairs = new();

    static OrderManager() {
        RegisterOrders();
    }

    private static void RegisterOrders() {
        RegisterOrder(NoOrder.Instance,               NoOrder.Instance);
        RegisterOrder(RandomOrder.Instance,           RandomOrder.Instance);
        RegisterOrder(item => item.Name,              "Name");
        RegisterOrder(item => item.OriginalTitle,     "OriginalTitle");
        RegisterOrder(item => item.PremiereDate,      "PremiereDate", "ReleaseDate", "Release Date");
        RegisterOrder(item => item.Path,              "Path");
        RegisterOrder(item => item.Container,         "Container");
        RegisterOrder(item => item.Tagline,           "Tagline");
        RegisterOrder(item => item.ChannelId,         "ChannelId");
        RegisterOrder(item => item.Id,                "Id");
        RegisterOrder(item => item.Width,             "Width");
        RegisterOrder(item => item.Height,            "Height");
        RegisterOrder(item => item.DateModified,      "DateModified");
        RegisterOrder(item => item.DateLastSaved,     "DateLastSaved");
        RegisterOrder(item => item.DateLastRefreshed, "DateLastRefreshed");
        RegisterOrder(item => item.MediaType,         "MediaType");
        RegisterOrder(item => item.SortName,          "SortName");
        RegisterOrder(item => item.ForcedSortName,    "ForcedSortName");
        RegisterOrder(item => item.EndDate,           "EndDate");
        RegisterOrder(item => item.Overview,          "Overview");
        RegisterOrder(item => item.ProductionYear,    "ProductionYear", "Year");

        RegisterOrder(item => item.CollectionName, "CollectionName", "BoxSet");
        RegisterOrder(item => item.HasSubtitles, "HasSubtitles");

        RegisterOrder(item => item.AiredSeasonNumber, "SeasonNumber", "AiredSeasonNumber");
        RegisterOrder(item => item.ParentIndexNumber, "IndexNumber",  "ParentIndexNumber", "ParentIndex");
        RegisterOrder(item => item.SeasonName,        "SeasonName",   "Season");
        RegisterOrder(item => item.SeriesName,        "SeriesName",   "Series");
    }

    private static void RegisterOrder<TKey>(Func<SortableBaseItem, TKey> keySelector, params string[] ids) {
        var ascending = new PropertyOrder<TKey>(keySelector, true, ids);
        var descending = new PropertyOrder<TKey>(keySelector, false, ids);

        foreach (var name in ids) {
            _orderPairs[name] = new(ascending, descending);
        }
    }

    private static void RegisterOrder<T>(T ascending, T descending) where T : Order {
        foreach (var name in ascending.Names()) {
            _orderPairs[name] = new(ascending, descending);
        }
    }

    public static Order GetOrder(OrderDto dto) => _orderPairs[dto.Name].Get(dto.Ascending);

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
