namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record DateTimeListExpressionValue : ExpressionValueList<DateTime> {
	public DateTimeListExpressionValue(IReadOnlyList<DateTime> value) : base(value) { }
}
