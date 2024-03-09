using static System.Text.Json.JsonSerializer;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

public class OrderByDtoJsonConverter : JsonConverter<OrderByDto>
{
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <inheritdoc />
    public override OrderByDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!JsonDocument.TryParseValue(ref reader, out var doc))
        {
            return null;
        }

        switch (doc.RootElement.ValueKind)
        {
            case JsonValueKind.Object: return ProcessObject(doc, options);
            case JsonValueKind.Array: return ProcessArray(doc, options);
            case JsonValueKind.String: return ProcessString(doc, options);
        }

        return null;
    }

    private static OrderByDto? ProcessObject(JsonDocument document, JsonSerializerOptions options)
    {
        string name;

        if (document.RootElement.TryGetProperty(nameof(OrderByDto.Name), out var nameElement))
        {
            name = nameElement.GetString() ?? string.Empty;
        }
        else
        {
            return null;
        }

        OrderByDto result = new() { Name = name, Ascending = true, };

        if (document.RootElement.TryGetProperty(nameof(OrderByDto.Ascending), out var ascendingElement))
        {
            result.Ascending = ascendingElement.GetBoolean();
        }

        if (!document.RootElement.TryGetProperty(nameof(OrderByDto.ThenBy), out var thenByElement))
        {
            return result;
        }

        foreach (var orderByElement in thenByElement.EnumerateArray())
        {
            var orderByParsed = orderByElement.Deserialize<OrderDto>(options);

            if (orderByParsed is not null)
            {
                result.ThenBy.Add(orderByParsed);
            }
        }

        return result;
    }

    private static OrderByDto ProcessString(JsonDocument document,
                                            JsonSerializerOptions options) =>
            new() { Name = document.RootElement.GetString() ?? string.Empty };

    private static OrderByDto ProcessArray(JsonDocument doc, JsonSerializerOptions options)
    {
        var rst = new OrderByDto();
        var elements = doc.RootElement.EnumerateArray().ToArray();
        var first = GetDto(elements.First(), options);
        rst.Name = first?.Name ?? string.Empty;
        rst.Ascending = first?.Ascending ?? true;

        foreach (var element in elements.Skip(1))
        {
            var ele = GetDto(element, options);

            if (ele is not null)
            {
                rst.ThenBy.Add(ele);
            }
        }

        return rst;
    }

    private static OrderDto? GetDto(JsonElement element, JsonSerializerOptions options) =>
            element.Deserialize<OrderDto>(options);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OrderByDto value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStartObject();
        writer.WriteString(nameof(value.Name), value.Name);
        writer.WriteBoolean(nameof(value.Ascending), value.Ascending);
        writer.WriteEndObject();

        foreach (var element in value.ThenBy)
        {
            Serialize(writer, element, options);
        }

        writer.WriteEndArray();
    }
}
