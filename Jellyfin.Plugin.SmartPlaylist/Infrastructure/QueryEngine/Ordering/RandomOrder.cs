namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Ordering;

public class RandomOrder : Order
{
    public static RandomOrder Instance { get; } = new();

    public RandomOrder() : base(true) { }

    public override IEnumerable<string> Names()
    {
        yield return "RandomOrder";
        yield return "Random";
        yield return "RNG";
        yield return "RND";
        yield return "DiceRoll";
    }

    public override IOrderedEnumerable<Operand> OrderBy(IEnumerable<Operand> items) =>
            items.OrderBy(_ => Random.Shared.Next());

    public override IOrderedEnumerable<Operand> ThenBy(IOrderedEnumerable<Operand> items) =>
            items.ThenBy(_ => Random.Shared.Next());
}
