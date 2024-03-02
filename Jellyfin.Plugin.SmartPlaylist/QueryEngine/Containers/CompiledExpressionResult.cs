namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

public record CompiledExpressionResult<T>(Func<T, bool> Expression,
                                          ParsedValueExpressionResult ParsedValueExpression,
                                          Exception? Exception = null);