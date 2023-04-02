namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Ordering;

public class NoOrder : Order {
	public const string ID = "NoOrder";

	public static NoOrder Instance { get; } = new();

	public NoOrder() : base(true) {
	}

	public override IEnumerable<string> Names() {
		yield return ID;
	}
}
