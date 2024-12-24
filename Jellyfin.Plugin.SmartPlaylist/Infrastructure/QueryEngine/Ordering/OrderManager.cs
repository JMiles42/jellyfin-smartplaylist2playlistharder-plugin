using System.Linq.Expressions;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Ordering;

public static class OrderManager
{
    internal static readonly Dictionary<string, OrderPair> _orderPairs = new();
    public static readonly OrderStack Default;

    static OrderManager()
    {
        RegisterOrders();
        Default = new(NoOrder.Instance);
    }

    public static bool IsValidOrderName(string name) => _orderPairs.ContainsKey(name);

    private static void RegisterOrders()
    {
        RegisterOrder(NoOrder.Instance, NoOrder.Instance);
        RegisterOrder(RandomOrder.Instance, RandomOrder.Instance);

        RegisterOrder(item => item.Name);
        RegisterOrder(item => item.SortName);
        RegisterOrder(item => item.ForcedSortName);
        RegisterOrder(item => item.Path);
        RegisterOrder(item => item.PremiereDate, "ReleaseDate", "Release Date");
        RegisterOrder(item => item.ProductionYearNotNull, nameof(Operand.ProductionYear), "Year");
        RegisterOrder(item => item.ParentIndexNumberNotNull, nameof(Operand.ParentIndexNumber), "ParentIndex");
        RegisterOrder(item => item.OriginalTitle);
        RegisterOrder(item => item.MediaType);
        RegisterOrder(item => item.Album);
        RegisterOrder(item => item.FolderPath);
        RegisterOrder(item => item.ContainingFolderPath);
        RegisterOrder(item => item.FileNameWithoutExtension);
        RegisterOrder(item => item.OfficialRating);
        RegisterOrder(item => item.Overview);
        RegisterOrder(item => item.Container);
        RegisterOrder(item => item.Tagline);
        RegisterOrder(item => item.ActorsLength);
        RegisterOrder(item => item.ArtistsLength);
        RegisterOrder(item => item.ComposersLength);
        RegisterOrder(item => item.DirectorsLength);
        RegisterOrder(item => item.GuestStarsLength);
        RegisterOrder(item => item.ProducersLength);
        RegisterOrder(item => item.WritersLength);
        RegisterOrder(item => item.GenresLength);
        RegisterOrder(item => item.StudiosLength);
        RegisterOrder(item => item.TagsLength);
        RegisterOrder(item => item.PathSegmentLength);
        RegisterOrder(item => item.Exists);
        RegisterOrder(item => item.HasSubtitles);
        RegisterOrder(item => item.IsFavoriteOrLiked);
        RegisterOrder(item => item.IsHorizontal);
        RegisterOrder(item => item.IsPlayed);
        RegisterOrder(item => item.IsSquare);
        RegisterOrder(item => item.IsVertical);
        RegisterOrder(item => item.DateCreated);
        RegisterOrder(item => item.DateLastRefreshed);
        RegisterOrder(item => item.DateLastSaved);
        RegisterOrder(item => item.DateModified);
        RegisterOrder(item => item.PlayedPercentage);
        RegisterOrder(item => item.LastPlayedDate);
        RegisterOrder(item => item.DaysSinceCreated, "DateSinceCreated");
        RegisterOrder(item => item.DaysSinceLastModified, "DateSinceLastModified");
        RegisterOrder(item => item.DaysSinceLastRefreshed, "DateSinceLastRefreshed");
        RegisterOrder(item => item.DaysSinceLastSaved, "DateSinceLastSaved");
        RegisterOrder(item => item.DaysSincePremiereDate, "DateSincePremiereDate");
        RegisterOrder(item => item.PlayedCount);
        RegisterOrder(item => item.AiredSeasonNumberNotNull, nameof(Operand.AiredSeasonNumber), "SeasonNumber", "Season Number");
        RegisterOrder(item => item.PlaybackPositionTicks);
        RegisterOrder(item => item.CollectionName, "BoxSet");
        RegisterOrder(item => item.SeasonName, "Season");
        RegisterOrder(item => item.SeriesName, "Series");
        RegisterOrder(item => item.AllText);
        RegisterOrder(item => item.Height);
        RegisterOrder(item => item.Width);
        RegisterOrder(item => item.ParentName);
        RegisterOrder(item => item.GrandparentName);
        RegisterOrder(item => item.CommunityRatingNotNull, nameof(Operand.CommunityRating));
        RegisterOrder(item => item.CriticRatingNotNull, nameof(Operand.CriticRating));
        RegisterOrder(item => item.EndDateNotNull, nameof(Operand.EndDate));
        RegisterOrder(item => item.ChannelId);
        RegisterOrder(item => item.Id);
        RegisterOrder(item => item.LengthNotNull, nameof(Operand.Length));
        RegisterOrder(item => item.LengthSecondsNotNull, nameof(Operand.LengthSeconds));
        RegisterOrder(item => item.LengthMinutesNotNull, nameof(Operand.LengthMinutes));
        RegisterOrder(item => item.LengthHoursNotNull, nameof(Operand.LengthHours));
        RegisterOrder(item => item.LengthTicksNotNull, nameof(Operand.LengthTicks));
        RegisterOrder(item => item.RunTimeTicksNotNull, nameof(Operand.RunTimeTicks));
    }

    private static void RegisterOrder<TKey>(Expression<Func<Operand, TKey>> keySelector) =>
            RegisterOrder(keySelector, []);

    private static void RegisterOrder<TKey>(Expression<Func<Operand, TKey>> keySelector,
                                            params string[] orderIds)
    {
        var hasMemberName = keySelector.TryGetMemberName(out var memberName);
        string[] ids;

        if (hasMemberName)
        {
            ids = new string[orderIds.Length + 1];
            orderIds.CopyTo(ids, 1);
            ids[0] = memberName;
        }
        else
        {
            ids = orderIds;
        }
        ValidateTypeIsOrderable(typeof(TKey), ids[0]);

        var compiled = keySelector.Compile();
        var ascending = new PropertyOrder<TKey>(compiled, true, ids);
        var descending = new PropertyOrder<TKey>(compiled, false, ids);

        var pair = new OrderPair(ascending, descending);

        if (hasMemberName)
        {
            RegisterOrder(memberName, pair);
        }

        foreach (var name in ids)
        {
            RegisterOrder(name, pair);
        }
    }

    private static void ValidateTypeIsOrderable(Type type, string memberName)
    {
        if (type.IsAssignableTo(typeof(IComparable)))
        {
            return;
        }

        throw new InvalidOperationException($"Type {type.GetCSharpName()} does not implement IComparable thus Operand.{memberName} cannot be ordered");
    }

    private static void RegisterOrder(string name, OrderPair pair) => _orderPairs[name] = pair;

    private static void RegisterOrder<T>(T ascending, T descending) where T : Order
    {
        foreach (var name in ascending.Names())
        {
            _orderPairs[name] = new(ascending, descending);
        }
    }

    public static Order GetOrder(IOrderDetails dto) => _orderPairs[dto.Name].Get(dto.Ascending);

    public static Order GetOrder(string name, bool ascending = true) => _orderPairs[name].Get(ascending);
}