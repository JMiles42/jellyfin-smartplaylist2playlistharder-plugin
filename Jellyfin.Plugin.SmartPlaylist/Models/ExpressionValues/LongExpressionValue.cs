namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record LongExpressionValue : ExpressionValue<long> {
	public LongExpressionValue(long value) : base(value) { }
}
