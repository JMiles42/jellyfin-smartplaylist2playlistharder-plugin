namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public abstract record ExpressionValue
{
    public abstract bool   IsSingleValue { get; }

    public abstract object SingleValue { get; }

    public abstract IEnumerable<object> GetValues();

    public static ExpressionValue Create<T>(T value) => new ExpressionValue<T>(value);

    public static ExpressionValue CreateList<T>(IReadOnlyList<T> value) => new ExpressionValueList<T>(value);

    public static ExpressionValue Create(bool value) => value ? BoolExpressionValue.True : BoolExpressionValue.False;
    public static ExpressionValue Create() => NullExpressionValue.Instance;
}

public record ExpressionValue<T>(T Value): ExpressionValue
{
    public override object SingleValue => Value;

    public override bool IsSingleValue => true;

    public override IEnumerable<object> GetValues() {
        yield return Value;
    }
}