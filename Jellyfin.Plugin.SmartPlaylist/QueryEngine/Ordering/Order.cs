namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public abstract class Order
{
	public bool Ascending { get; }

	protected Order(bool ascending) => Ascending = ascending;

	public virtual IOrderedEnumerable<Operand> OrderBy(IEnumerable<Operand> items) => items.OrderBy(a => a);

	public virtual IOrderedEnumerable<Operand> ThenBy(IOrderedEnumerable<Operand> items) => items.ThenBy(a => a);

	public abstract IEnumerable<string> Names();
}
