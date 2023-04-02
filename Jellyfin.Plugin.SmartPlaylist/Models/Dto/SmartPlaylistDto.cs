using Jellyfin.Data.Enums;

namespace Jellyfin.Plugin.SmartPlaylist.Models.Dto;

[Serializable]
public class SmartPlaylistDto
{
    public static readonly BaseItemKind[] SupportedItemDefault = {
            BaseItemKind.Audio,
            BaseItemKind.Episode,
            BaseItemKind.Movie
    };

    public string Id { get; set; }

    public string Name { get; set; }

    public string FileName { get; set; }

    public string User { get; set; }

    public List<ExpressionSet> ExpressionSets { get; set; }

    public int MaxItems { get; set; }

    public OrderByDto Order { get; set; }

    public BaseItemKind[] SupportedItems { get; set; }

    public SmartPlaylistDto Validate()
    {
        SupportedItems ??= SupportedItemDefault;

        return this;
    }

}
