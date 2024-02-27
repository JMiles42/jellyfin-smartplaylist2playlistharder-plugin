namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record TimeSpanListExpressionValue : ExpressionValueList<TimeSpan> {
	public TimeSpanListExpressionValue(IReadOnlyList<TimeSpan> value) : base(value) { }
}
