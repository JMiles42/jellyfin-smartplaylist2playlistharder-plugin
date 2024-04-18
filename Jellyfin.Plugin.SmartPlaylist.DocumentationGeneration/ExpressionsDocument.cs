namespace Jellyfin.Plugin.SmartPlaylist.DocumentationGeneration;

public static class ExpressionsDocument
{
	public static void Generate()
	{
		var document = new MdDocument();
		document.Root.Add(new MdHeading("How to use the expressions", 1));
		document.Root.Add(new MdParagraph("This document is WIP, and needs details added."));

		document.Save(Program.OUTPUTPATH + "Expressions.md");
	}
}
