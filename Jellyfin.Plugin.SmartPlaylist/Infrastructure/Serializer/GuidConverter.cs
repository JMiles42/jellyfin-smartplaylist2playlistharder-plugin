using static System.Text.Json.JsonSerializer;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

public class GuidConverter: JsonConverter<Guid>
{

	/// <inheritdoc />
	public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert,
							  JsonSerializerOptions options) =>
			Guid.Parse(reader.GetString());

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Guid value,
							   JsonSerializerOptions options) =>
			writer.WriteStringValue(value.ToString());
}

public class GuidNullableConverter: JsonConverter<Guid?>
{

	/// <inheritdoc />
	public override Guid? Read(ref Utf8JsonReader    reader,
							   Type                  typeToConvert,
							   JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}

		return Guid.Parse(reader.GetString());
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter        writer,
							   Guid?                 value,
							   JsonSerializerOptions options) =>
			Serialize(writer, value);
}
