namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public abstract class Order
{
	public bool Ascending { get; }

	protected Order(bool ascending) => Ascending = ascending;

	public virtual IOrderedEnumerable<SortableBaseItem> OrderBy(IEnumerable<SortableBaseItem> items) =>
			items.OrderBy(a => a);

	public virtual IOrderedEnumerable<SortableBaseItem> ThenBy(IOrderedEnumerable<SortableBaseItem> items) =>
			items.ThenBy(a => a);

	public abstract IEnumerable<string> Names();
}
