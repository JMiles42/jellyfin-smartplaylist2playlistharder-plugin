using static System.Text.Json.JsonSerializer;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

public sealed class ExpressionValueJsonConverter : JsonConverter<ExpressionValue>
{
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <inheritdoc />
    public override ExpressionValue Read(ref Utf8JsonReader reader,
                                         Type typeToConvert,
                                         JsonSerializerOptions options)
    {
        if (!JsonDocument.TryParseValue(ref reader, out var doc))
        {
            return NullExpressionValue.Instance;
        }

        return doc.RootElement.ValueKind switch
        {
            JsonValueKind.Array => ProcessArray(doc),
            JsonValueKind.String => ProcessString(doc.RootElement.GetString()!),
            JsonValueKind.Number => ExpressionValue.Create(doc.RootElement.GetInt32()),
            JsonValueKind.True => ExpressionValue.Create(true),
            JsonValueKind.False => ExpressionValue.Create(false),
            JsonValueKind.Null => ExpressionValue.Create(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private static ExpressionValue ProcessString(string value)
    {
        if (value.StartsWith("$(") && value.EndsWith(')'))
        {
            return LinkedExpressionValue.Create(value);
        }

        return ExpressionValue.Create(value);
    }

    private static ExpressionValue ProcessArray(JsonDocument doc)
    {
        var length = doc.RootElement.GetArrayLength();

        var array = doc.RootElement.EnumerateArray().ToArray();

        var allTypes = array.Select(a => a.ValueKind).Distinct().ToArray();

        if (allTypes.Length != 1)
        {
            return ExpressionValue.CreateList(array.Select(a => a.GetRawText()).ToArray());
        }

        return allTypes[0] switch
        {
            JsonValueKind.Array => ExpressionValue.CreateList(array.Select(a => a.GetRawText()).ToArray()),
            JsonValueKind.String => ExpressionValue.CreateList(array.Select(a => a.GetString()!).ToArray()),
            JsonValueKind.Number => ExpressionValue.CreateList(array.Select(a => a.GetInt32()).ToArray()),
            JsonValueKind.True => ExpressionValue.Create(true),
            JsonValueKind.False => ExpressionValue.Create(false),
            JsonValueKind.Null => ExpressionValue.Create(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer,
                               ExpressionValue value,
                               JsonSerializerOptions options)
    {
        if (value is LinkedExpressionValue linkedExpression)
        {
            writer.WriteRawValue(Serialize(linkedExpression.VarSource, options));
            return;
        }

        if (value.IsSingleValue)
        {
            writer.WriteRawValue(Serialize(value.SingleValue, options));
        }
        else
        {
            writer.WriteStartArray();

            foreach (var element in value.GetValues())
            {
                Serialize(writer, element, options);
            }

            writer.WriteEndArray();
        }
    }
}