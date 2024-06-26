﻿namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default,
							 WriteIndented = true,
							 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(SmartPlaylistDto))]
[JsonSerializable(typeof(OrderByDto))]
[JsonSerializable(typeof(OrderDto))]
public partial class SmartPlaylistDtoJsonContext: JsonSerializerContext
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
		WriteIndented          = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
	});
}
