namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record TimeOnlyListExpressionValue : ExpressionValueList<TimeOnly> {
	public TimeOnlyListExpressionValue(IReadOnlyList<TimeOnly> value) : base(value) { }
}
