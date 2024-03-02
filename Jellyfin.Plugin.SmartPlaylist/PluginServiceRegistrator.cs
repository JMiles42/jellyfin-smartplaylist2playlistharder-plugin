namespace Jellyfin.Plugin.SmartPlaylist;

/// <summary>
///     Register webhook services.
/// </summary>
public class PluginServiceRegistrator: IPluginServiceRegistrator
{
	/// <inheritdoc />
	public void RegisterServices(IServiceCollection serviceCollection) =>
			serviceCollection.AddScoped<SmartPlaylistsRefreshAll>();
}
