namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public abstract record ExpressionValue
{
    public abstract bool IsSingleValue { get; }
    public abstract object SingleValue { get; }

    public abstract IEnumerable<object> GetValues();

    public static ExpressionValue<T> Create<T>(T value) => new(value);

    public static ExpressionValueList<T> CreateList<T>(IReadOnlyList<T> value) => new(value);

    public static ExpressionValue Create() => NullExpressionValue.Instance;

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsSingleValue)
        {
            return SingleValue.ToString();
        }

        StringBuilder sb = new();
        sb.Append('[');
        bool first = true;
        foreach (var a in GetValues())
        {
            if (!first)
            {
                sb.Append(", ");
                first = false;
            }

            if (a is string)
            {
                sb.Append('"');
                sb.Append(a);
                sb.Append('"');
            }
            else
            {
                sb.Append(a);
            }
        }
        sb.Append(']');
        return sb.ToString();
    }
}

public record ExpressionValue<T>(T Value) : ExpressionValue
{
    public override object SingleValue => Value;

    public override bool IsSingleValue => true;

    public override IEnumerable<object> GetValues()
    {
        yield return Value;
    }
}