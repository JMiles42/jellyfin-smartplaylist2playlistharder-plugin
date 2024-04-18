namespace Jellyfin.Plugin.SmartPlaylist.DocumentationGeneration;

internal class Program
{
	public const string OUTPUTPATH = @"";
	private static void Main(string[] args)
	{
		OrderDocument.Generate();
		ExpressionsDocument.Generate();
		OperandDocument.Generate();
	}
}
