using System.Runtime.CompilerServices;
using FluentAssertions;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;


public class ParsingFileTests
{
    private static readonly string _appFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    private static readonly string _dataPath = Path.Combine(_appFolder, "Data", "IO");

    private static async Task<SmartPlaylistDto> LoadFile([CallerMemberName] string filename = "") {
        var fullPath = Path.Combine(_dataPath, filename + ".json");
        var contents = await SmartPlaylistStore.LoadPlaylistAsync(fullPath);
        contents.Should().NotBeNull();
        return contents!;
    }

    private static async Task SaveFile(SmartPlaylistDto data, [CallerMemberName] string filename = "") {
        var fullPath = Path.Combine(_dataPath, filename + ".output.json");
        await SmartPlaylistStore.SaveAsync(data, fullPath);
    }

    [Fact]
    public async Task Simple_With_StringComparison_AsInt()
    {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Simple_With_StringComparison_AsString()
    {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Simple_Without_StringComparison()
    {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.CurrentCulture);
    }

    [Fact]
    public async Task Simple_Without_SupportedItems()
    {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.CurrentCulture);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    [Fact]
    public async Task TargetValue_AsList() {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    [Fact]
    public async Task TargetValue_AsString() {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    [Fact]
    public async Task TargetValue_AsList_SaveIsEqual() {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
        await SaveFile(dto);
    }

    [Fact]
    public async Task TargetValue_AsString_SaveIsEqual() {
        var dto = await LoadFile();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
        await SaveFile(dto);
    }
}
