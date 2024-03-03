namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

public class OrderDtoJsonConverter: JsonConverter<OrderDto>
{
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <inheritdoc />
	public override OrderDto? Read(ref Utf8JsonReader    reader,
								   Type                  typeToConvert,
								   JsonSerializerOptions options)
	{
		if (!JsonDocument.TryParseValue(ref reader, out var doc))
		{
			return null;
		}

		return doc.RootElement.ValueKind switch
		{
			JsonValueKind.Object => ProcessObject(doc, options),
			JsonValueKind.Array  => ProcessArray(doc, options),
			JsonValueKind.String => ProcessString(doc, options),
			_                    => null,
		};
	}

	private static OrderDto ProcessObject(JsonDocument          document,
										  JsonSerializerOptions jsonSerializerOptions)
	{
		var name = document.RootElement.GetProperty(nameof(OrderDto.Name)).GetString()!;

		if (document.RootElement.TryGetProperty(nameof(OrderDto.Ascending), out var asc) &&
			asc.ValueKind is JsonValueKind.True or JsonValueKind.False)
		{
			return new() { Name = name, Ascending = asc.GetBoolean(), };
		}

		return new() { Name = name, };
	}

	private static OrderDto ProcessString(JsonDocument          document,
										  JsonSerializerOptions jsonSerializerOptions) =>
			new() { Name = document.RootElement.GetString()! };

	private static OrderDto? ProcessArray(JsonDocument          doc,
										  JsonSerializerOptions jsonSerializerOptions)
	{
		if (doc.RootElement.GetArrayLength() <= 2)
		{
			return null;
		}

		var array  = doc.RootElement.EnumerateArray().ToArray();
		var first  = array[0];
		var second = array[1];

		return GetOrderDtoFrom2LengthArray(first,  second) ??
			   GetOrderDtoFrom2LengthArray(second, first);
	}

	private static OrderDto? GetOrderDtoFrom2LengthArray(JsonElement first,
														 JsonElement second)
	{
		if (first.ValueKind is JsonValueKind.String && second.ValueKind is JsonValueKind.False or JsonValueKind.True)
		{
			return new() { Name = first.GetString()!, Ascending = second.GetBoolean(), };
		}

		return null;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter        writer,
							   OrderDto              value,
							   JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString(nameof(value.Name), value.Name);
		writer.WriteBoolean(nameof(value.Ascending), value.Ascending);
		writer.WriteEndObject();
	}
}
