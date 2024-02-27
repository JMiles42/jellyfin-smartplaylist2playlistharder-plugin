namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record StringExpressionValue : ExpressionValue<string> {
	public StringExpressionValue(string value) : base(value) { }
}
