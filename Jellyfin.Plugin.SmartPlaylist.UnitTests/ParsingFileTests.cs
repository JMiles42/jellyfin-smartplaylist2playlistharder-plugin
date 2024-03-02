using System.Runtime.CompilerServices;
using FluentAssertions;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;


public class ParsingFileTests
{
    private static readonly string _appFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    private static readonly string _dataPath = Path.Combine(_appFolder, "Data", "IO");

    private static PlaylistIoData LoadFile([CallerMemberName] string filename = "") {
        var filename_ext = filename + ".json";
        var contents     = SmartPlaylistManager.LoadPlaylist(Path.Combine(_dataPath, filename_ext), filename_ext);
        contents.Should().NotBeNull();
        return contents!;
    }

    private static void SaveFile(SmartPlaylistDto data, [CallerMemberName] string filename = "") {
        var file = Path.Combine(_dataPath, filename + ".output.json");
        SmartPlaylistManager.SavePlaylist(file, data);
    }

    [Fact]
    public void Simple_With_StringComparison_AsInt()
    {
        var io  = LoadFile();
        var dto = io.SmartPlaylist;
        dto.Should().NotBeNull();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Simple_With_StringComparison_AsString()
    {
        var io  = LoadFile();
        var dto = io.SmartPlaylist;
        dto.Should().NotBeNull();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Simple_Without_StringComparison()
    {
        var io  = LoadFile();
        var dto = io.SmartPlaylist;
        dto.Should().NotBeNull();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.CurrentCulture);
    }

    [Fact]
    public void Simple_Without_SupportedItems()
    {
        var io  = LoadFile();
        var dto = io.SmartPlaylist;
        dto.Should().NotBeNull();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.CurrentCulture);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    [Fact]
    public void TargetValue_AsList() {
        var io  = LoadFile();
        var dto = io.SmartPlaylist;
        dto.Should().NotBeNull();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    [Fact]
    public void TargetValue_AsString() {
        var io  = LoadFile();
        var dto = io.SmartPlaylist;
        dto.Should().NotBeNull();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    [Fact]
    public void TargetValue_AsList_SaveIsEqual() {
        var io  = LoadFile();
        var dto = io.SmartPlaylist;
        dto.Should().NotBeNull();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
        SaveFile(dto);
    }

    [Fact]
    public void TargetValue_AsString_SaveIsEqual() {
        var io  = LoadFile();
        var dto = io.SmartPlaylist;
        dto.Should().NotBeNull();
        dto.Name.Should().BeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.Should().Be(OperandMember.Directors);
        dto.ExpressionSets[0].Expressions[0].StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.Should().NotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
        SaveFile(dto);
    }
}
