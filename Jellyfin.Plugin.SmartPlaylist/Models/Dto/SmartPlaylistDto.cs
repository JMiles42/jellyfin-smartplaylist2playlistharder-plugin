namespace Jellyfin.Plugin.SmartPlaylist.Models.Dto;

[Serializable]
public sealed class SmartPlaylistDto
{
    public static readonly BaseItemKind[] SupportedItemDefault =
    [
        BaseItemKind.Audio, BaseItemKind.Episode, BaseItemKind.Movie,
    ];

    [JsonConverter(typeof(GuidNullableConverter))]
    public Guid? Id { get; set; }

    public string? Name { get; set; }

    [JsonIgnore]
    public string? FileName { get; set; }

    public string? User { get; set; }

    public List<ExpressionSet>? ExpressionSets { get; set; }

    public MatchMode Match { get; set; } = MatchMode.Any;

    public int MaxItems { get; set; }

    [JsonConverter(typeof(OrderByDtoJsonConverter))]
    public OrderByDto? Order { get; set; }

    private BaseItemKind[] supportedItems = SupportedItemDefault;

    public BaseItemKind[] SupportedItems
    {
        get => supportedItems;
        set
        {
            supportedItems = value ?? SupportedItemDefault;
            Array.Sort(supportedItems);
        }
    }

    public bool IsReadonly { get; set; }
}