namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record TimeSpanExpressionValue : ExpressionValue<TimeSpan> {
	public TimeSpanExpressionValue(TimeSpan value) : base(value) { }
}
