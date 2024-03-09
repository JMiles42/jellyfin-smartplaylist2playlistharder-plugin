using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;

namespace Jellyfin.Plugin.SmartPlaylist;

public class PluginServiceRegistrator: IPluginServiceRegistrator
{
	/// <inheritdoc />
	public void RegisterServices(IServiceCollection serviceCollection)
	{
		serviceCollection.AddTransient<ISmartPlaylistsRefreshAll, SmartPlaylistsRefreshAll>();
		serviceCollection.AddTransient<IPlaylistApplicationPaths, PlaylistApplicationPaths>();
		serviceCollection.AddTransient<ISmartPlaylistManager,     SmartPlaylistManager>();
		serviceCollection.AddTransient<SmartPlaylistsRefreshJob>();
		serviceCollection.AddTransient<PlaylistUpdaterFactory>();
		serviceCollection.AddTransient<SmartPlaylistsRefreshJobFactory>();
		serviceCollection.AddTransient<OperandFactory>();
		serviceCollection.AddTransient<ISmartPlaylistPluginConfiguration>((sp) => new SmartPlaylistPluginConfigReadonly(SmartPlaylistPlugin.Instance!.Configuration));
	}
}
