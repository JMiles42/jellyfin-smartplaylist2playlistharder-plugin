namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record IntListExpressionValue : ExpressionValueList<int> {
	public IntListExpressionValue(IReadOnlyList<int> value) : base(value) { }
}
