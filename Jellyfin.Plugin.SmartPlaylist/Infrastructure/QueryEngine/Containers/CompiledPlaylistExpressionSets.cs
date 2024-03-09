namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Containers;

public class CompiledPlaylistExpressionSets
{
    public List<CompiledExpressionSet<Operand>> CompiledExpressionSets { get; set; } = new();
}
