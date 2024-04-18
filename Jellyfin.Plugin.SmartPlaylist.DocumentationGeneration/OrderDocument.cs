using Jellyfin.Plugin.SmartPlaylist.Extensions;

namespace Jellyfin.Plugin.SmartPlaylist.DocumentationGeneration;

public static class OrderDocument
{
	public static void Generate()
	{
		var document = new MdDocument();
		document.Root.Add(new MdHeading("Here are all the ways to order playlists", 1));
		document.Root.Add(new MdParagraph("Ordering playlists is done via a stack of Order objects, that primarily follow the property names of the Operand type." + Environment.NewLine +
										  "There are some that don't relate to properties, such as the random one, which can be used to basically shuffle the playlists order." + Environment.NewLine));

		var table = new MdTable(GetMdTableRowForOrder("Name/Operand Property Name", "Alias", "Object Type"));

		foreach (var pair in OrderManager._orderPairs.Values.DistinctBy(a => a.Get().Names().First())) {
			var asc = pair.Get();

			var names   = asc.Names().ToArray();
			var name    = names.First();
			var aliases = string.Join(", ", names.Skip(1));

			if (asc is IPropertyOrder order) {
				table.Add(GetMdTableRowForOrder(name, aliases, order.KeyType.GetCSharpName()));

			}
			else {
				table.Add(GetMdTableRowForOrder(name, aliases, "N/A"));
			}
		}

		document.Root.Add(table);
		document.Save(Program.OUTPUTPATH + "Ordering.md");
	}

	private static MdTableRow GetMdTableRowForOrder(string name, string aliases, string objectType) => new(name, aliases, objectType);
}
