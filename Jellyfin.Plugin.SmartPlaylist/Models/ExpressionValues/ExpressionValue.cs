namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public abstract record ExpressionValue
{
    public abstract bool   IsSingleValue { get; }

    public abstract object SingleValue { get; }

    public abstract IEnumerable<object> GetValues();

    public static ExpressionValue Create<T>(T value) {
        return value switch {
                DateTime v => Create(v),
                DateOnly v => Create(v),
                TimeOnly v => Create(v),
                TimeSpan v => Create(v),
                double v => Create(v),
                long v => Create(v),
                int v => Create(v),
                string v => Create(v),
                bool v => Create(v),
                null => NullExpressionValue.Instance,
                _ => new ExpressionValue<T>(value),
        };
    }

    public static ExpressionValue CreateList<T>(IReadOnlyList<T> value) {
        return value switch {
                IReadOnlyList<DateTime> v => CreateList(v),
                IReadOnlyList<DateOnly> v => CreateList(v),
                IReadOnlyList<TimeOnly> v => CreateList(v),
                IReadOnlyList<TimeSpan> v => CreateList(v),
                IReadOnlyList<double> v => CreateList(v),
                IReadOnlyList<long> v => CreateList(v),
                IReadOnlyList<int> v => CreateList(v),
                IReadOnlyList<string> v => CreateList(v),
                IReadOnlyList<bool> v => CreateList(v),
                null => NullExpressionValue.Instance,
                _ => new ExpressionValueList<T>(value),
        };
    }
    public static ExpressionValue Create(object value, Type type) {
        if (type == typeof(string)) {
            return Create((string)value);
        }
        if (type == typeof(bool)) {
            return Create((bool)value);
        }
        if (type == typeof(long)) {
            return Create((long)value);
        }
        if (type == typeof(int)) {
            return Create((int)value);
        }
        if (type == typeof(double)) {
            return Create((double)value);
        }
        if (type == typeof(DateTime)) {
            return Create((DateTime)value);
        }
        if (type == typeof(DateOnly)) {
            return Create((DateOnly)value);
        }
        if (type == typeof(TimeOnly)) {
            return Create((TimeOnly)value);
        }
        if (type == typeof(TimeSpan)) {
            return Create((TimeSpan)value);
        }

        return new ExpressionValue<object>(value);
    }

    public static ExpressionValue Create(DateTime value) => new DateTimeExpressionValue(value);
    public static ExpressionValue Create(DateOnly value) => new DateOnlyExpressionValue(value);
    public static ExpressionValue Create(TimeOnly value) => new TimeOnlyExpressionValue(value);
    public static ExpressionValue Create(TimeSpan value) => new TimeSpanExpressionValue(value);
    public static ExpressionValue Create(double value) => new DoubleExpressionValue(value);
    public static ExpressionValue Create(long value) => new LongExpressionValue(value);
    public static ExpressionValue Create(int value) => new IntExpressionValue(value);
    public static ExpressionValue Create(string value) => new StringExpressionValue(value);
    public static ExpressionValue Create(bool value) => value ? BoolExpressionValue.True : BoolExpressionValue.False;

    public static ExpressionValue CreateList(IReadOnlyList<DateTime> value) => new DateTimeListExpressionValue(value);
    public static ExpressionValue CreateList(IReadOnlyList<DateOnly> value) => new DateOnlyListExpressionValue(value);
    public static ExpressionValue CreateList(IReadOnlyList<TimeOnly> value) => new TimeOnlyListExpressionValue(value);
    public static ExpressionValue CreateList(IReadOnlyList<TimeSpan> value) => new TimeSpanListExpressionValue(value);
    public static ExpressionValue CreateList(IReadOnlyList<double>   value) => new DoubleListExpressionValue(value);
    public static ExpressionValue CreateList(IReadOnlyList<long>      value) => new LongListExpressionValue(value);
    public static ExpressionValue CreateList(IReadOnlyList<int>      value) => new IntListExpressionValue(value);
    public static ExpressionValue CreateList(IReadOnlyList<string>   value) => new StringListExpressionValue(value);
    public static ExpressionValue CreateList(IReadOnlyList<bool>     value) => new BoolListExpressionValue(value);
    public static ExpressionValue Create()                              => NullExpressionValue.Instance;
}

public record ExpressionValue<T>: ExpressionValue
{
    public T Value       { get; init; }

    public override object SingleValue => Value;

    public override bool IsSingleValue => true;

    public ExpressionValue(T value) {
        Value = value;
    }

    public override IEnumerable<object> GetValues() {
        yield return Value;
    }
}