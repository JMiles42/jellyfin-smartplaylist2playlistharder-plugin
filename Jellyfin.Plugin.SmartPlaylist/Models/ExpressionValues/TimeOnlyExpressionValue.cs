namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record TimeOnlyExpressionValue : ExpressionValue<TimeOnly> {
	public TimeOnlyExpressionValue(TimeOnly value) : base(value) { }
}