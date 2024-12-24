namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default,
                             WriteIndented = true,
                             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(SmartPlaylistDto))]
[JsonSerializable(typeof(OrderByDto))]
[JsonSerializable(typeof(OrderDto))]
public sealed partial class SmartPlaylistDtoJsonContext : JsonSerializerContext
{
    public static SmartPlaylistDtoJsonContext WithConverters = new(new()
    {
        Converters =
        {
            new JsonStringEnumConverter(allowIntegerValues: true),
            new ExpressionValueJsonConverter(),
            new OrderByDtoJsonConverter(),
            new OrderDtoJsonConverter(),
            new GuidConverter(),
            new GuidNullableConverter(),
        },
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    });



    public static SmartPlaylistDto? Deserialize(Stream utf8Json)
    {
        var result = JsonSerializer.Deserialize(utf8Json, WithConverters.SmartPlaylistDto);

        if (result is not null)
        {
            FixLinkedVars(result);
        }

        return result;
    }

    private static void FixLinkedVars(SmartPlaylistDto result)
    {
        if (result.ExpressionSets is null or { Count: 0 })
        {
            return;
        }

        if (result.ExpressionVars is null or { Count: 0 })
        {
            return;
        }

        foreach (var jsonPropertyInfo in result.ExpressionSets)
        {
            foreach (var smartPlExpression in jsonPropertyInfo.Expressions)
            {
                if (smartPlExpression.TargetValue is LinkedExpressionValue linkedExpression)
                {
                    var key = linkedExpression.VarName;

                    if (result.ExpressionVars.TryGetValue(key, out var expressionValue))
                    {
                        linkedExpression.LinkValue(expressionValue);
                    }
                }
            }
        }
    }
}