namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Containers;

public sealed class CompiledPlaylistExpressionSets
{
    public List<CompiledExpressionSet<Operand>> CompiledExpressionSets { get; set; } = [];
}