namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

public class CompiledPlaylistExpressionSets
{
    public List<CompiledExpressionSet<Operand>> CompiledExpressionSets { get; set; } = new();
}
