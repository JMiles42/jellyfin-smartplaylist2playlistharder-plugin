namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public abstract record ExpressionValueList : ExpressionValue
{
    public abstract int Count { get; }

    public override bool IsSingleValue => Count == 1;
}

public record ExpressionValueList<T>(IReadOnlyList<T> Value) : ExpressionValueList
{
    public override int Count => Value.Count;

#pragma warning disable CS8603 // Possible null reference return.
    public override object SingleValue => SingleValueT;
#pragma warning restore CS8603 // Possible null reference return.

    public T SingleValueT => Value[0];

    public override IEnumerable<object> GetValues() => Value.Cast<object>();
}
