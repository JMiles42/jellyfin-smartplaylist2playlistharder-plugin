namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record DoubleExpressionValue : ExpressionValue<double> {
	public DoubleExpressionValue(double value) : base(value) { }
}
