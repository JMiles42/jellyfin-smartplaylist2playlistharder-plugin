using Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonSerializer;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

public class ExpressionValueJsonConverter : JsonConverter<ExpressionValue>
{
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <inheritdoc />
    public override ExpressionValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (!JsonDocument.TryParseValue(ref reader, out var doc)) {
            return NullExpressionValue.Instance;
        }

        if (doc.RootElement.ValueKind is JsonValueKind.Array) {
            return ProcessArray(doc);
        }
        switch (doc.RootElement.ValueKind) {
            case JsonValueKind.Undefined: break;
            case JsonValueKind.Object:    break;
            case JsonValueKind.Array:
                return ProcessArray(doc);
            case JsonValueKind.String:
                return ExpressionValue.Create(doc.RootElement.GetString()!);
            case JsonValueKind.Number:
                return ExpressionValue.Create(doc.RootElement.GetInt32());
            case JsonValueKind.True:
                return ExpressionValue.Create(true);
            case JsonValueKind.False:
                return ExpressionValue.Create(false);
            case JsonValueKind.Null:
                return ExpressionValue.Create();
        }

        throw new ArgumentOutOfRangeException();
    }

    private static ExpressionValue ProcessArray(JsonDocument doc) {
        var length = doc.RootElement.GetArrayLength();

        var array = doc.RootElement.EnumerateArray().ToArray();

        var allTypes = array.Select(a => a.ValueKind).Distinct().ToArray();

        if (allTypes.Length == 1) {
            switch (allTypes[0]) {
                case JsonValueKind.Undefined: break;
                case JsonValueKind.Object:    break;
                case JsonValueKind.Array:
                    return ExpressionValue.CreateList(array.Select(a => a.GetRawText()).ToArray());
                case JsonValueKind.String:
                    return ExpressionValue.CreateList(array.Select(a => a.GetString()!).ToArray());
                case JsonValueKind.Number:
                    return ExpressionValue.CreateList(array.Select(a => a.GetInt32()).ToArray());
                case JsonValueKind.True:  return ExpressionValue.Create(true);
                case JsonValueKind.False: return ExpressionValue.Create(false);
                case JsonValueKind.Null:  return ExpressionValue.Create();
                default:                  throw new ArgumentOutOfRangeException();
            }
        }

        return ExpressionValue.CreateList(array.Select(a => a.GetRawText()).ToArray());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ExpressionValue value, JsonSerializerOptions options)
    {
        if (value.IsSingleValue) {
            writer.WriteRawValue(Serialize(value.SingleValue, options));
        }
        else {
            writer.WriteStartArray();
            foreach (var element in value.GetValues()) {
                Serialize(writer, element, options);
            }

            writer.WriteEndArray();
        }
    }
}
