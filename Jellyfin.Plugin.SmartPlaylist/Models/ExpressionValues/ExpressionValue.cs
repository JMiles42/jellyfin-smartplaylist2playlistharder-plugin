namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public abstract record ExpressionValue
{
	public abstract bool IsSingleValue { get; }

	public abstract object SingleValue { get; }

	public abstract IEnumerable<object> GetValues();

	public static ExpressionValue<T> Create<T>(T value) => new(value);

	public static ExpressionValueList<T> CreateList<T>(IReadOnlyList<T> value) => new(value);

	public static ExpressionValue Create() => NullExpressionValue.Instance;
}

public record ExpressionValue<T>(T Value): ExpressionValue
{
	public override object SingleValue => Value;

	public override bool IsSingleValue => true;

	public override IEnumerable<object> GetValues()
	{
		yield return Value;
	}
}
