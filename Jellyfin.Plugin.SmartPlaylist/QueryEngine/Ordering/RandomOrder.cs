namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public class RandomOrder : Order {
	public static RandomOrder Instance { get; } = new();

	public RandomOrder() : base(true) {

	}

	public override IEnumerable<string> Names() {
		yield return "RandomOrder";
		yield return "Random";
		yield return "RNG";
		yield return "RND";
		yield return "DiceRoll";
	}

	public override IOrderedEnumerable<SortableBaseItem> OrderBy(IEnumerable<SortableBaseItem> items) {
		return items.OrderBy(_ => Random.Shared.Next());
	}

	public override IOrderedEnumerable<SortableBaseItem> ThenBy(IOrderedEnumerable<SortableBaseItem> items) => items.ThenBy(_ => Random.Shared.Next());
}
