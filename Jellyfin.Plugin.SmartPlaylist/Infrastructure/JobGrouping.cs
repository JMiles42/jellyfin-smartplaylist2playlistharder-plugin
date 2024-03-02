using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public record JobGrouping(User? User, BaseItemKind[]? Kinds);
