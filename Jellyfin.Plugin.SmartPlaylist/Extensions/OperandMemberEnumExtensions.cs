namespace Jellyfin.Plugin.SmartPlaylist.Extensions;

public static class OperandMemberEnumExtensions
{
	private static readonly ConcurrentDictionary<OperandMember, string> _cache = new();

	public static string ToStringFast(this OperandMember states)
	{
		if (_cache.TryGetValue(states, out var value))
		{
			return value;
		}

		return _cache[states] = states.ToString();
	}
}
