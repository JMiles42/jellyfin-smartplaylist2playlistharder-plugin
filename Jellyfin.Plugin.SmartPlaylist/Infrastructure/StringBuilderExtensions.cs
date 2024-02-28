using System.Text;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public static class StringBuilderExtensions
{
	public static void AppendLineIfNotNullOrEmpty(this StringBuilder sb, string? value) {
		if (string.IsNullOrEmpty(value)) {
			return;
		}

		sb.AppendLine(value);
	}
}
