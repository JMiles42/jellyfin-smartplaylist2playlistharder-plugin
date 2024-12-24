namespace Jellyfin.Plugin.SmartPlaylist.Api.Models;

internal sealed class SmartPlaylistLastRunDetailsComparer : IComparer<SmartPlaylistLastRunDetails>
{
    public static readonly SmartPlaylistLastRunDetailsComparer Instance = new();

    public int Compare(SmartPlaylistLastRunDetails? left, SmartPlaylistLastRunDetails? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (right is null)
        {
            return 1;
        }

        if (left is null)
        {
            return -1;
        }

        var leftIsSuccess = left.Status == SmartPlaylistLastRunDetails.SUCCESS;
        var rightIsSuccess = right.Status == SmartPlaylistLastRunDetails.SUCCESS;

        return leftIsSuccess switch
        {
            true when rightIsSuccess => StringComparer.OrdinalIgnoreCase.Compare(left.PlaylistId, right.PlaylistId),
            true when !rightIsSuccess => -1,
            false when rightIsSuccess => 1,
            _ => StringComparer.OrdinalIgnoreCase.Compare(left.PlaylistId, right.PlaylistId) +
                 StringComparer.OrdinalIgnoreCase.Compare(left.Status, right.Status),
        };
    }
}