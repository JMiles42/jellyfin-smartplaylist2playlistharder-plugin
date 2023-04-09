using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public abstract class Order {
	public bool Ascending { get; }

	protected Order(bool ascending) => Ascending = ascending;

	public virtual IOrderedEnumerable<BaseItem> OrderBy(IEnumerable<BaseItem> items) => items.OrderBy(a => a);
	public virtual IOrderedEnumerable<BaseItem> ThenBy(IOrderedEnumerable<BaseItem> items) => items.ThenBy(a => a);

	public abstract IEnumerable<string> Names();
}
