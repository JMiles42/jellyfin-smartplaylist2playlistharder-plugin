namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record IntExpressionValue : ExpressionValue<int> {
	public IntExpressionValue(int value) : base(value) { }
}
