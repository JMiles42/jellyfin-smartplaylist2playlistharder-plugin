namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record DateOnlyListExpressionValue : ExpressionValueList<DateOnly> {
	public DateOnlyListExpressionValue(IReadOnlyList<DateOnly> value) : base(value) { }
}
