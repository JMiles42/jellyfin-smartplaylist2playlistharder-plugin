namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine;

public static class OperandHelper
{
    internal static Dictionary<string, Type> properties;

    static OperandHelper()
    {
        var type = typeof(Operand);
        var props = type.GetProperties();

        properties = new(props.Length);

        foreach (var propertyInfo in props)
        {
            properties[propertyInfo.Name] = propertyInfo.PropertyType;
        }
    }

    public static bool IsValidPropertyName(string name) => properties.ContainsKey(name);

    public static Type? GetPropertyType(string name) => properties.TryGetValue(name, out var rst) ? rst : null;

    public static IEnumerable<string> GetPropertyNames() => properties.Keys;
}