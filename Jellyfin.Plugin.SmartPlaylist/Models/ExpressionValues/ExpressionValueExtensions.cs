namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public static class ExpressionValueExtensions
{
    public static ExpressionValue<T> ToExpressionValue<T>(this T value) => ExpressionValue.Create(value);

    public static ExpressionValueList<T> ToExpressionValue<T>(this IReadOnlyList<T> value) =>
            ExpressionValue.CreateList(value);

    public static ExpressionValueList<T> ToExpressionValue<T>(this List<T> value) => ExpressionValue.CreateList(value);
}