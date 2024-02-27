namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record BoolListExpressionValue : ExpressionValueList<bool> {
	public BoolListExpressionValue(IReadOnlyList<bool> value) : base(value) { }
}
