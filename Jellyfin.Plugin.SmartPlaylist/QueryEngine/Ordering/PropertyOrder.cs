namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public class PropertyOrder<TKey>: Order
{
	public string[] Ids { get; }

	public Func<Operand, TKey> KeySelector { get; }

	public PropertyOrder(Func<Operand, TKey> keySelector, bool ascending, params string[] ids):
			base(ascending)
	{
		Ids         = ids;
		KeySelector = keySelector;
	}

	public override IOrderedEnumerable<Operand> OrderBy(IEnumerable<Operand> items) =>
			Ascending? items.OrderBy(KeySelector) : items.OrderByDescending(KeySelector);

	public override IOrderedEnumerable<Operand> ThenBy(IOrderedEnumerable<Operand> items) =>
			Ascending? items.ThenBy(KeySelector) : items.ThenByDescending(KeySelector);

	public override IEnumerable<string> Names() => Ids;
}
