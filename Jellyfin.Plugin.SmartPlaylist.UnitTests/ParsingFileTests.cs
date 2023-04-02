using System.Runtime.CompilerServices;
using FluentAssertions;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;


public class ParsingFileTests
{
    private static readonly string _appFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    private static readonly string _dataPath = Path.Combine(_appFolder, "Data", "IO");

    [Fact]
    public async Task Simple_With_StringComparison_AsInt()
    {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().BeEquivalentTo("Directors");
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Simple_With_StringComparison_AsString()
    {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().BeEquivalentTo("Directors");
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Simple_Without_StringComparison()
    {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().BeEquivalentTo("Directors");
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.CurrentCulture);
    }

    [Fact]
    public async Task Simple_Without_SupportedItems()
    {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().BeEquivalentTo("Directors");
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.CurrentCulture);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    private static async Task<SmartPlaylistDto> LoadFile([CallerMemberName] string filename = "")
    {
        var fullPath = Path.Combine(_dataPath, filename + ".json");
        var contents = await SmartPlaylistStore.LoadPlaylistAsync(fullPath);
        contents.Should().NotBeNull();
        return contents!;
    }
}
