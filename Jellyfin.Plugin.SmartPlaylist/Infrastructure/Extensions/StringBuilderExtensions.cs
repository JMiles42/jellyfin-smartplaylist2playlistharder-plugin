namespace Jellyfin.Plugin.SmartPlaylist.Extensions;

public static class StringBuilderExtensions
{
	public static void AppendLineIfNotNullOrEmpty(this StringBuilder sb, string? value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return;
		}

		sb.AppendLine(value);
	}
}
