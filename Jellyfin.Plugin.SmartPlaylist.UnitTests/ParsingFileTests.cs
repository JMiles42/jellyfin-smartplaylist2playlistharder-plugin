using Jellyfin.Plugin.SmartPlaylist.Configuration;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure.ProcessEngine;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Model;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using System.Runtime.CompilerServices;
using Shouldly;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;


public class ParsingFileTests
{
    private static readonly string _appFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    private static readonly string _dataPath = Path.Combine(_appFolder, "Data");

    private readonly ISmartPlaylistManager _smartPlaylistManager;

    private class TestHelper : ISmartPlaylistPluginConfiguration
    {
        /// <inheritdoc />
        public int PlaylistSorterThreadCount => 2;

        /// <inheritdoc />
        public bool PlaylistDetailedErrors => true;

        public bool PlaylistBatchedProcessing => true;

        /// <inheritdoc />
        public bool AlwaysSaveFile => false;

        /// <inheritdoc />
        public bool BackupFileOnSave => false;

        /// <inheritdoc />
        public string PlaylistFolderName => "IO";

        /// <inheritdoc />
        public string PlaylistBackupFolderName => SmartPlaylistPluginConfiguration.PLAYLIST_BACKUP_FOLDER_NAME;
    }

    public ParsingFileTests()
    {
        var config = new TestHelper();
        var paths = new PlaylistApplicationPaths(_dataPath, config);
        _smartPlaylistManager = new SmartPlaylistManager(paths, config);
    }

    private PlaylistProcessRunData LoadFile([CallerMemberName] string filename = "")
    {
        var filename_ext = filename + ".json";
        var contents = _smartPlaylistManager.LoadPlaylist(filename_ext);
        contents.ShouldNotBeNull();
        return contents!;
    }

    [Theory]
    [InlineData("Simple_With_StringComparison_AsInt")]
    [InlineData("Simple_With_StringComparison_AsString")]
    [InlineData("Simple_Without_StringComparison")]
    [InlineData("Simple_Without_SupportedItems")]
    [InlineData("TargetValue_AsList")]
    [InlineData("TargetValue_AsList_SaveIsEqual")]
    [InlineData("TargetValue_AsString")]
    [InlineData("TargetValue_AsString_SaveIsEqual")]
    [InlineData("Vars_AsList")]
    [InlineData("Vars_AsList_SaveIsEqual")]
    [InlineData("Vars_AsString")]
    [InlineData("Vars_AsString_SaveIsEqual")]
    public async Task FileLoads_Successfully(string filename)
    {
        var file = LoadFile(filename);

        var settings = new VerifySettings();
        settings.UseDirectory("snapshots");
        settings.UseParameters(filename);

        await Verify(file, settings);
    }

    private void SaveFile(SmartPlaylistDto data, [CallerMemberName] string filename = "") => _smartPlaylistManager.SavePlaylist(filename + ".output.json", data);

    [Fact]
    public void Simple_With_StringComparison_AsInt()
    {
        var io = LoadFile();
        var dto = io.SmartPlaylist;
        dto.ShouldNotBeNull();
        dto.Name.ShouldBeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.ShouldBe(nameof(Operand.Directors));
        dto.ExpressionSets[0].Expressions[0].StringComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Simple_With_StringComparison_AsString()
    {
        var io = LoadFile();
        var dto = io.SmartPlaylist;
        dto.ShouldNotBeNull();
        dto.Name.ShouldBeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.ShouldBe(nameof(Operand.Directors));
        dto.ExpressionSets[0].Expressions[0].StringComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Simple_Without_StringComparison()
    {
        var io = LoadFile();
        var dto = io.SmartPlaylist;
        dto.ShouldNotBeNull();
        dto.Name.ShouldBeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.ShouldBe(nameof(Operand.Directors));
        dto.ExpressionSets[0].Expressions[0].StringComparison.ShouldBe(StringComparison.CurrentCulture);
    }

    [Fact]
    public void Simple_Without_SupportedItems()
    {
        var io = LoadFile();
        var dto = io.SmartPlaylist;
        dto.ShouldNotBeNull();
        dto.Name.ShouldBeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.ShouldBe(nameof(Operand.Directors));
        dto.ExpressionSets[0].Expressions[0].StringComparison.ShouldBe(StringComparison.CurrentCulture);
		dto.SupportedItems.ShouldNotBeNull();
		dto.SupportedItems.ShouldBeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    [Fact]
    public void TargetValue_AsList()
    {
        var io = LoadFile();
        var dto = io.SmartPlaylist;
        dto.ShouldNotBeNull();
        dto.Name.ShouldBeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.ShouldBe(nameof(Operand.Directors));
        dto.ExpressionSets[0].Expressions[0].StringComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
        //dto.SupportedItems.ShouldNotBeNullOrEmpty().And.HaveCount(3).And.BeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
		dto.SupportedItems.ShouldNotBeNull();
		dto.SupportedItems.ShouldNotBeEmpty();
		dto.SupportedItems.ShouldBeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);

	}

	[Fact]
    public void TargetValue_AsString()
    {
        var io = LoadFile();
        var dto = io.SmartPlaylist;
        dto.ShouldNotBeNull();
        dto.Name.ShouldBeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.ShouldBe(nameof(Operand.Directors));
        dto.ExpressionSets[0].Expressions[0].StringComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
        dto.SupportedItems.ShouldNotBeNull();
		dto.SupportedItems.ShouldBeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
    }

    [Fact]
    public void TargetValue_AsList_SaveIsEqual()
    {
        var io = LoadFile();
        var dto = io.SmartPlaylist;
        dto.ShouldNotBeNull();
        dto.Name.ShouldBeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.ShouldBe(nameof(Operand.Directors));
        dto.ExpressionSets[0].Expressions[0].StringComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
		dto.SupportedItems.ShouldNotBeNull();
		dto.SupportedItems.ShouldBeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
		SaveFile(dto);
    }

    [Fact]
    public void TargetValue_AsString_SaveIsEqual()
    {
        var io = LoadFile();
        var dto = io.SmartPlaylist;
        dto.ShouldNotBeNull();
        dto.Name.ShouldBeEquivalentTo("OP Strats");
        dto.ExpressionSets[0].Expressions[0].MemberName.ShouldBe(nameof(Operand.Directors));
        dto.ExpressionSets[0].Expressions[0].StringComparison.ShouldBe(StringComparison.OrdinalIgnoreCase);
		dto.SupportedItems.ShouldNotBeNull();
		dto.SupportedItems.ShouldBeEquivalentTo(SmartPlaylistDto.SupportedItemDefault);
		SaveFile(dto);
    }
}
