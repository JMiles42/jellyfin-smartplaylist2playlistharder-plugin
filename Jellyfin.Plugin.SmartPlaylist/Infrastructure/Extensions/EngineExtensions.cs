// ReSharper disable NullableWarningSuppressionIsUsed
namespace Jellyfin.Plugin.SmartPlaylist.Extensions;

internal static class EngineExtensions
{
    public static readonly MethodInfo StringArrayContainsSubstringMethodInfo =
        typeof(EngineExtensions)
            .GetMethod(nameof(StringArrayContainsSubstring),
                BindingFlags.Static | BindingFlags.Public)!;

    public static readonly MethodInfo StringArrayContainsMethodInfo =
        typeof(EngineExtensions)
            .GetMethod(nameof(StringArrayContains),
                BindingFlags.Static | BindingFlags.Public)!;

    public static bool StringArrayContainsSubstring(this IReadOnlyCollection<string> left, string right,
        StringComparison stringComparison) =>
        left.Any(a => a.Contains(right, stringComparison));

    public static bool StringArrayContains(this IReadOnlyCollection<string> left, string right,
        StringComparison stringComparison) =>
        left.Any(a => a.Equals(right, stringComparison));

    public static LinqExpression ToConstantExpressionAsType<T>(this object value)
    {
        if (value is T tValue)
        {
            return LinqExpression.Constant(tValue);
        }

        return LinqExpression.Constant(Convert.ChangeType(value, typeof(T)));
    }

    public static LinqExpression ToConstantExpressionAsType(this object value, Type type)
    {
        if (value?.GetType() == type)
        {
            return LinqExpression.Constant(value);
        }

        return LinqExpression.Constant(Convert.ChangeType(value, type));
    }

    public static LinqExpression ToConstantExpression<T>(this T value) => LinqExpression.Constant(value);
}