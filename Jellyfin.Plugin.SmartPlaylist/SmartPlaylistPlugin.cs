using Jellyfin.Plugin.SmartPlaylist.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.SmartPlaylist;

public class SmartPlaylistPlugin : BasePlugin<SmartPlaylistPluginConfiguration>, IHasWebPages {

	public static SmartPlaylistPlugin? Instance { get; private set; }

	public override Guid Id => Guid.Parse(SmartPlaylistConsts.PLUGIN_GUID);

	public override string Name => SmartPlaylistConsts.PLUGIN_NAME;

	public override string Description => SmartPlaylistConsts.PLUGIN_DESCRIPTION;

	public SmartPlaylistPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer) {
		Instance = this;
	}

	public IEnumerable<PluginPageInfo> GetPages() {
		return new[] {
				new PluginPageInfo {
						Name                 = "configPage.html",
						EmbeddedResourcePath = $"{GetType().Namespace}.Pages.configPage.html",
				},
				new PluginPageInfo {
						//MenuSection          = "server",
						DisplayName          = SmartPlaylistConsts.PLUGIN_NAME,
						EnableInMainMenu     = true,
						EmbeddedResourcePath = $"{GetType().Namespace}.Pages.pluginDataPage.html",
						Name                 = "pluginDataPage.html",
						MenuIcon = "playlist_add_check_circle"
				},
		};
	}
}
