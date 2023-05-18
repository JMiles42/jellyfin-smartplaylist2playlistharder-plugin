using FluentAssertions;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;

public class SmartPlaylistTest {
    [Fact]
    public void DtoToSmartPlaylist() {
        var dto = new SmartPlaylistDto {
            Id = "87ccaa10-f801-4a7a-be40-46ede34adb22",
            Name = "Foo",
            User = "Rob"
        };

        var es = new ExpressionSet { Expressions = new() { new("foo", "bar", "biz") } };

        dto.ExpressionSets = new() { es };

        dto.Order = new() {
            Name = "Release Date",
            Ascending = false,
        };

        var smartPlaylist = new Models.SmartPlaylist(dto);

        smartPlaylist.MaxItems.Should().Be(0);
        smartPlaylist.Id.Should().Be("87ccaa10-f801-4a7a-be40-46ede34adb22");
        smartPlaylist.Name.Should().Be("Foo");
        smartPlaylist.User.Should().Be("Rob");
        smartPlaylist.ExpressionSets[0].Expressions[0].MemberName.Should().Be("foo");
        smartPlaylist.ExpressionSets[0].Expressions[0].Operator.Should().Be("bar");
        smartPlaylist.ExpressionSets[0].Expressions[0].TargetValue.Should().Be("biz");
        smartPlaylist.Order.Order[0].Names().First().Should().Be("PremiereDate");
    }

    [Fact]
    public void DtoToSmartPlaylist_CanGetExtensionExpression() {
        var dto = new SmartPlaylistDto {
            Id = "87ccaa10-f801-4a7a-be40-46ede34adb22",
            Name = "Foo",
            User = "Rob"
        };

        var es = new ExpressionSet { Expressions = new() { new("Directors", "StringListContainsSubstring", "CGP") } };

        dto.ExpressionSets = new() { es };

        dto.Order = new() {
            Name = "Release Date",
            Ascending = false,
        };

        var smartPlaylist = new Models.SmartPlaylist(dto);
        var compiled = smartPlaylist.GetCompiledRules();

        smartPlaylist.MaxItems.Should().Be(0);
    }

    [Fact]
    public void DtoToSmartPlaylist_CanGetStringCaseInSensitive() {
        var dto = new SmartPlaylistDto {
            Id = "87ccaa10-f801-4a7a-be40-46ede34adb22",
            Name = "Foo",
            User = "Rob"
        };

        var es = new ExpressionSet { Expressions = new() { new("Name", "Contains", "CGP", false, StringComparison.OrdinalIgnoreCase) } };

        dto.ExpressionSets = new() { es };

        dto.Order = new() {
            Name = "Release Date",
            Ascending = false,
        };

        var smartPlaylist = new Models.SmartPlaylist(dto);
        var compiled = smartPlaylist.GetCompiledRules();

        smartPlaylist.MaxItems.Should().Be(0);
    }
}
