using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public abstract class Order {
	public bool Ascending { get; }

	protected Order(bool ascending) => Ascending = ascending;

	public virtual IEnumerable<BaseItem> OrderBy(IEnumerable<BaseItem> items) => items;

	public abstract IEnumerable<string> Names();
}
