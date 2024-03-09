namespace Jellyfin.Plugin.SmartPlaylist.DocumentationGeneration;

internal class Program {
	private static void Main(string[] args)
	{
		OrderDocument.Generate();
		ExpressionsDocument.Generate();
	}
}
