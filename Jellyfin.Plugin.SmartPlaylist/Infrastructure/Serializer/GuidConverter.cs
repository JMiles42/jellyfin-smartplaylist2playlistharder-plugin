using static System.Text.Json.JsonSerializer;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

public sealed class GuidConverter : JsonConverter<Guid>
{
    /// <inheritdoc />
    public override Guid Read(ref Utf8JsonReader reader,
                              Type typeToConvert,
                              JsonSerializerOptions options) => reader.GetString() is { } str ? Guid.Parse(str) : Guid.Empty;

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer,
                               Guid value,
                               JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
}

public sealed class GuidNullableConverter : JsonConverter<Guid?>
{
    /// <inheritdoc />
    public override Guid? Read(ref Utf8JsonReader reader,
                               Type typeToConvert,
                               JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return reader.GetString() is { } str ? Guid.Parse(str) : null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer,
                               Guid? value,
                               JsonSerializerOptions options) =>
            Serialize(writer, value);
}