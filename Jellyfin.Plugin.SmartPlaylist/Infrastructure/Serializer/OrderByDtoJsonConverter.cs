using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using static System.Text.Json.JsonSerializer;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

public class OrderByDtoJsonConverter : JsonConverter<OrderByDto>
{
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <inheritdoc />
    public override OrderByDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (!JsonDocument.TryParseValue(ref reader, out var doc)) {
            return null;
        }

        switch (doc.RootElement.ValueKind) {
            case JsonValueKind.Object:
                return doc.RootElement.Deserialize<OrderByDto>(options);
            case JsonValueKind.Array:
                return ProcessArray(doc);
            case JsonValueKind.String:
                return ProcessString(doc);
        }

        return null;
    }

    private static OrderByDto ProcessString(JsonDocument document) => new() { Name = document.RootElement.GetString()! };

    private static OrderByDto ProcessArray(JsonDocument doc) {
        var rst      = new OrderByDto();
        var elements = doc.RootElement.EnumerateArray().ToArray();
        var first    = GetDto(elements.First());
        rst.Name = first.Name;
        rst.Ascending = first.Ascending;

        foreach (var element in elements.Skip(1)) {
            rst.ThenBy.Add(GetDto(element));
        }

        return rst;
    }

   static OrderDto GetDto(JsonElement element) {
        if (element.ValueKind == JsonValueKind.Object) {
            return element.Deserialize<OrderDto>();
        }

        if (element.ValueKind == JsonValueKind.String) {
            return new () { Name = element.GetString()! };
        }

        return null;
    }

   /// <inheritdoc />
   public override void Write(Utf8JsonWriter writer, OrderByDto value, JsonSerializerOptions options) {
       if (value is { Ascending: true, ThenBy.Count: 0 }) {
           writer.WriteStringValue(value.Name);

           return;
       }

       writer.WriteStartObject();
       writer.WriteString(nameof(value.Name), value.Name);

       if (!value.Ascending) {
           writer.WriteBoolean(nameof(value.Ascending), value.Ascending);
       }

       if (value.ThenBy.Count > 0) {
           writer.WriteStartArray(nameof(value.ThenBy));

           foreach (var element in value.ThenBy) {
               Serialize(writer, element, options);
           }

           writer.WriteEndArray();
       }

       writer.WriteEndObject();
   }
}
