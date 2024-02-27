using System.Text.Json.Serialization;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Serializer;

namespace Jellyfin.Plugin.SmartPlaylist.Models.Dto;

[Serializable]
public class SmartPlaylistDto {
	public static readonly BaseItemKind[] SupportedItemDefault = {
			BaseItemKind.Audio,
			BaseItemKind.Episode,
			BaseItemKind.Movie,
	};

	public string Id { get; set; }

	public string Name { get; set; }

	[JsonIgnore]
	public string FileName { get; set; }

	public string User { get; set; }

	public List<ExpressionSet> ExpressionSets { get; set; }

	public int MaxItems { get; set; }

	[JsonConverter(typeof(OrderByDtoJsonConverter))]
	public OrderByDto Order { get; set; }

	public BaseItemKind[] SupportedItems { get; set; }

	public bool IsReadonly { get; set; }

	public SmartPlaylistDto Validate() {
		SupportedItems ??= SupportedItemDefault;
		Array.Sort(SupportedItems);

		return this;
	}

}
