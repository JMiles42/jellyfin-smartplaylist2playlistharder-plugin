namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record ObjectListExpressionValue : ExpressionValueList<object> {
	public ObjectListExpressionValue(IReadOnlyList<object> value) : base(value) { }
}
