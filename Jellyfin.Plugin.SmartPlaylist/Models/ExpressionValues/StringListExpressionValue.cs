namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record StringListExpressionValue : ExpressionValueList<string> {
	public StringListExpressionValue(IReadOnlyList<string> value) : base(value) { }
}
