namespace Jellyfin.Plugin.SmartPlaylist.Extensions;

public static class StringExtensions
{
	public static string ValueOrEmpty(this string? value) => value ?? string.Empty;
}
