namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public class PropertyOrder<TKey>: Order
{
	public string[] Ids { get; }

	public Func<SortableBaseItem, TKey> KeySelector { get; }

	public PropertyOrder(Func<SortableBaseItem, TKey> keySelector, bool ascending, params string[] ids):
			base(ascending)
	{
		Ids         = ids;
		KeySelector = keySelector;
	}

	public override IOrderedEnumerable<SortableBaseItem> OrderBy(IEnumerable<SortableBaseItem> items) =>
			Ascending? items.OrderBy(KeySelector) : items.OrderByDescending(KeySelector);

	public override IOrderedEnumerable<SortableBaseItem> ThenBy(IOrderedEnumerable<SortableBaseItem> items) =>
			Ascending? items.ThenBy(KeySelector) : items.ThenByDescending(KeySelector);

	public override IEnumerable<string> Names() => Ids;
}
