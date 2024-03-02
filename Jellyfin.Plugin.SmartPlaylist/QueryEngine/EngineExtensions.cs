namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

internal static class EngineExtensions
{
	public static readonly MethodInfo StringArrayContainsSubstringMethodInfo =
			typeof(EngineExtensions)!.GetMethod(nameof(StringArrayContainsSubstring),
												BindingFlags.Static | BindingFlags.Public);

	public static readonly MethodInfo StringArrayContainsMethodInfo =
			typeof(EngineExtensions)!.GetMethod(nameof(StringArrayContains), BindingFlags.Static | BindingFlags.Public);

	private static readonly DateTime _origin = new(1970,
												   1,
												   1,
												   0,
												   0,
												   0,
												   0,
												   DateTimeKind.Utc);

	public static bool StringArrayContainsSubstring(this IReadOnlyCollection<string> l,
													string                           r,
													StringComparison                 stringComparison) =>
			l.Any(a => a.Contains(r, stringComparison));

	public static bool StringArrayContains(this IReadOnlyCollection<string> l,
										   string                           r,
										   StringComparison                 stringComparison) =>
			l.Any(a => a.Equals(r, stringComparison));

	public static linqExpression InvertIfTrue(this linqExpression expression, bool invert) =>
			invert? linqExpression.Not(expression) : expression;

	public static linqExpression ToConstantExpressionAsType<T>(this object value)
	{
		if (value is T tValue)
		{
			return linqExpression.Constant(tValue);
		}

		return linqExpression.Constant(Convert.ChangeType(value, typeof(T)));
	}

	public static linqExpression ToConstantExpressionAsType(this object value, Type type)
	{
		if (value?.GetType() == type)
		{
			return linqExpression.Constant(value);
		}

		return linqExpression.Constant(Convert.ChangeType(value, type));
	}

	public static linqExpression ToConstantExpression<T>(this T value) => linqExpression.Constant(value);

	public static double ConvertToUnixTimestamp(this DateTime date)
	{
		var diff = date.ToUniversalTime() - _origin;

		return Math.Floor(diff.TotalSeconds);
	}
}
