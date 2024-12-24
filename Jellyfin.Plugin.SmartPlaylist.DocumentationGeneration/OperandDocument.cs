using Jellyfin.Plugin.SmartPlaylist.Extensions;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine;
using MediaBrowser.Controller.Entities;
using System.Reflection;

namespace Jellyfin.Plugin.SmartPlaylist.DocumentationGeneration;

public sealed class OperandDocument
{
    private const string NOT_EXIST_TEXT = "null";
    public static void Generate()
    {
        var document = new MdDocument();
        document.Root.Add(new MdHeading("Here are all the ways to order playlists", 1));

        var table = new MdTable(GetMdTableRowForOrder("Operand Name",
                                                      "Operand Type",
                                                      "BaseItem Name",
                                                      "BaseItem Type"));

        var pairs = GetPairs().ToList();
        foreach (var pair in pairs)
        {

            table.Add(GetMdTableRowForOrder(pair.Operand?.Name ?? NOT_EXIST_TEXT,
                                            pair.Operand?.PropertyType.GetCSharpName() ?? NOT_EXIST_TEXT,
                                            pair.BaseItem?.Name ?? NOT_EXIST_TEXT,
                                            pair.BaseItem?.PropertyType.GetCSharpName() ?? NOT_EXIST_TEXT
                                            ));
        }

        document.Root.Add(table);
        document.Save(Program.OUTPUTPATH + "OperandProperties.md");
    }

    private static IEnumerable<(PropertyInfo? Operand, PropertyInfo? BaseItem)> GetPairs()
    {
        var opProps = typeof(Operand).GetProperties();
        var biProps = typeof(BaseItem).GetProperties();

        var opDic = opProps.ToDictionary(a => a.Name);
        var biDic = biProps.ToDictionary(a => a.Name);


        foreach (var propertyInfo in opDic)
        {
            if (biDic.ContainsKey(propertyInfo.Key))
            {
                yield return (propertyInfo.Value, biDic[propertyInfo.Key]);
            }
            else
            {
                yield return (propertyInfo.Value, null);
            }
        }

        foreach (var propertyInfo in biDic.Where(a => !opDic.ContainsKey(a.Key)))
        {
            yield return (null, propertyInfo.Value);
        }
    }




    private static MdTableRow GetMdTableRowForOrder(string operandName,
                                                    string operandType,
                                                    string baseItemName,
                                                    string baseItemType) =>
            new(operandName, operandType, baseItemName, baseItemType);
}
