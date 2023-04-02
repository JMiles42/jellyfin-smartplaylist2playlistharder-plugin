using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.SmartPlaylist.Models.Dto;

public class OrderDto
{
    [JsonPropertyOrder(0)]
    public string Name { get; set; }

    [JsonPropertyOrder(1)]
    public bool Ascending { get; set; } = true;
}

public class OrderByDto : OrderDto
{
    [JsonPropertyOrder(3)]
    public List<OrderDto> ThenBy { get; set; } = new();
}
