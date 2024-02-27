namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record DateTimeExpressionValue : ExpressionValue<DateTime> {
	public DateTimeExpressionValue(DateTime value) : base(value) { }
}