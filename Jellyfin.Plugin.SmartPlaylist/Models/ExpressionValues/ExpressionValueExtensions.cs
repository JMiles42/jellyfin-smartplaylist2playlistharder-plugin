namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public static class ExpressionValueExtensions
{
	public static ExpressionValue ToExpressionValue<T>(this T value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue<T>(this IReadOnlyList<T> value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue<T>(this List<T> value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this bool value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this double value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this string value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this int value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this DateTime value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this DateOnly value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this TimeOnly value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this TimeSpan value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this IReadOnlyList<bool> value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this IReadOnlyList<double> value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this IReadOnlyList<string> value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this IReadOnlyList<int> value) => ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this IReadOnlyList<DateTime> value) =>
			ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this IReadOnlyList<DateOnly> value) =>
			ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this IReadOnlyList<TimeOnly> value) =>
			ExpressionValue.Create(value);

	public static ExpressionValue ToExpressionValue(this IReadOnlyList<TimeSpan> value) =>
			ExpressionValue.Create(value);
}
