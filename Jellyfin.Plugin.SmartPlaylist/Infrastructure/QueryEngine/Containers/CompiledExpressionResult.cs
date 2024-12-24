namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.Containers;

public record CompiledExpressionResult<T>(Func<T, bool> Expression,
                                          ParsedValueExpressionResult ParsedValueExpression,
                                          Exception? Exception = null);