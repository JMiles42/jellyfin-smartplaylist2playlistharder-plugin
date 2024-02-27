namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record DateOnlyExpressionValue : ExpressionValue<DateOnly> {
	public DateOnlyExpressionValue(DateOnly value) : base(value) { }
}