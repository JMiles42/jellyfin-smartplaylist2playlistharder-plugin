namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record NullExpressionValue: ExpressionValue
{
	/// <inheritdoc />
	public override bool IsSingleValue => true;

	/// <inheritdoc />
	public override object SingleValue => null;

	/// <inheritdoc />
	public override IEnumerable<object> GetValues() {
		yield return null;
	}

	public static NullExpressionValue Instance { get; } = new ();
}
