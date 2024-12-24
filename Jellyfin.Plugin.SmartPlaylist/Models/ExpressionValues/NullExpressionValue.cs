namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

#pragma warning disable CS8603 // Possible null reference return.
public record NullExpressionValue : ExpressionValue
{
    public static NullExpressionValue Instance { get; } = new();

    /// <inheritdoc />
    public override bool IsSingleValue => true;
    /// <inheritdoc />
    public override object SingleValue => null;

    /// <inheritdoc />
    public override IEnumerable<object> GetValues()
    {
        yield return null;
    }
}
#pragma warning restore CS8603 // Possible null reference return.
