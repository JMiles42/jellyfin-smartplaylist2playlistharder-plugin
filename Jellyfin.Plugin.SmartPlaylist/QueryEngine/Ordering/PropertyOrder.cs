using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public class PropertyOrder<TKey>: Order
{
	public string[] Ids { get; }

	public Func<BaseItem, TKey> KeySelector { get; }

	public PropertyOrder(Func<BaseItem, TKey> keySelector, bool ascending, params string[] ids): base(ascending) {
		Ids         = ids;
		KeySelector = keySelector;
	}

	public override IEnumerable<BaseItem> OrderBy(IEnumerable<BaseItem> items) =>
			Ascending? items.OrderBy(KeySelector) : items.OrderByDescending(KeySelector);

	public override IEnumerable<string> Names() => Ids;
}
