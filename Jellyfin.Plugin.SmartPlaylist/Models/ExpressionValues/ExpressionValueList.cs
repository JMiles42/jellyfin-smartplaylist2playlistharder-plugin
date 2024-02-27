namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public abstract record ExpressionValueList : ExpressionValue{
    public abstract int Count { get; }
    public override bool IsSingleValue => Count == 1;
}

public record ExpressionValueList<T> : ExpressionValueList {
    public          IReadOnlyList<T> Value         { get; init; }

    public override int Count => Value.Count;

    public override object SingleValue  => SingleValueT;
    public          T      SingleValueT => Value.First();

    public ExpressionValueList(IReadOnlyList<T> value)
    {
        Value = value;
    }

    public override IEnumerable<object> GetValues()
    {
        return Value.Cast<object>();
    }
}
