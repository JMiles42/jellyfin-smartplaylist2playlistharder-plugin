using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.SmartPlaylist;

/// <summary>
/// Register webhook services.
/// </summary>
public class PluginServiceRegistrator: IPluginServiceRegistrator
{
	/// <inheritdoc />
	public void RegisterServices(IServiceCollection serviceCollection) {
		serviceCollection.AddScoped<SmartPlaylistsRefreshAll>();
	}
}

