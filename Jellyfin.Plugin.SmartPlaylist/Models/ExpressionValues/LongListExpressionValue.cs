namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record LongListExpressionValue : ExpressionValueList<long> {
	public LongListExpressionValue(IReadOnlyList<long> value) : base(value) { }
}
