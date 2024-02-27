namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record ObjectExpressionValue : ExpressionValue<object> {
	public ObjectExpressionValue(object value) : base(value) { }
}
