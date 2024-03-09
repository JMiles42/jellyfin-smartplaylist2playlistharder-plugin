using FluentAssertions;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;
using Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

namespace Jellyfin.Plugin.SmartPlaylist.UnitTests;

public class SmartPlaylistTest {
    static readonly Guid PlaylistId = Guid.Parse("87ccaa10-f801-4a7a-be40-46ede34adb22");
    [Fact]
    public void DtoToSmartPlaylist() {
        var dto = new SmartPlaylistDto {
            Id   = PlaylistId,
            Name = "Foo",
            User = "Rob",
        };

        var es = new ExpressionSet { Expressions = new() { new(nameof(Operand.Name), "bar", ExpressionValue.Create("biz")) } };

        dto.ExpressionSets = new() { es };

        dto.Order = new() {
            Name = "Release Date",
            Ascending = false,
        };

        var smartPlaylist = new Models.SmartPlaylist(dto);

        smartPlaylist.MaxItems.Should().Be(0);
        smartPlaylist.Id.Should().Be(PlaylistId);
        smartPlaylist.Name.Should().Be("Foo");
        smartPlaylist.User.Should().Be("Rob");
        smartPlaylist.ExpressionSets[0].Expressions[0].MemberName.Should().Be(nameof(Operand.Name));
        smartPlaylist.ExpressionSets[0].Expressions[0].Operator.Should().Be("bar");
        smartPlaylist.ExpressionSets[0].Expressions[0].TargetValue.Should().Be("biz".ToExpressionValue());
        smartPlaylist.Order.Order[0].Names().First().Should().Be("PremiereDate");
    }

    [Fact]
    public void DtoToSmartPlaylist_CanGetExtensionExpression() {
        var dto = new SmartPlaylistDto {
            Id   = PlaylistId,
            Name = "Foo",
            User = "Rob"
        };

        var es = new ExpressionSet { Expressions = new() { new(nameof(Operand.Directors), "StringListContainsSubstring", "CGP".ToExpressionValue()) } };

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
            Id   = PlaylistId,
            Name = "Foo",
            User = "Rob",
        };

        var es = new ExpressionSet {
                Expressions = new() {
                        new(nameof(Operand.Name),
                            "Contains",
                            "CGP".ToExpressionValue(),
                            MatchMode.All,
                            false,
                            StringComparison.OrdinalIgnoreCase)
                }
        };

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
