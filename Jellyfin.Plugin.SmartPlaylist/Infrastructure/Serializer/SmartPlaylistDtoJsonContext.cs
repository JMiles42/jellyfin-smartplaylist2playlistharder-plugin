using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(SmartPlaylistDto))]
public partial class SmartPlaylistDtoJsonContext : JsonSerializerContext
{
	public static SmartPlaylistDtoJsonContext WithConverters = new(new() {
			Converters = {
					new JsonStringEnumConverter(allowIntegerValues:true),
					new ExpressionValueJsonConverter(),
					//new OrderByDtoJsonConverter(),
					new OrderDtoJsonConverter(),
			},
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
	});
}
