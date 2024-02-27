namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record DoubleListExpressionValue : ExpressionValueList<double> {
	public DoubleListExpressionValue(IReadOnlyList<double> value) : base(value) { }
}
