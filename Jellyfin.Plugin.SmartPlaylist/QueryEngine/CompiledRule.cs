namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

public class CompiledRule
{
	public List<List<Func<Operand, bool>>> CompiledRuleSets { get; set; } = new();
}
