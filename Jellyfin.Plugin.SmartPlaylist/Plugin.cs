using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.SmartPlaylist;

public class Plugin : BasePlugin<BasePluginConfiguration>, IHasWebPages {

	public static Plugin Instance { get; private set; }

	public override Guid Id => Guid.Parse("5A63FE63-765F-43F7-A129-661E839F83D5");

	public override string Name => "Smart Playlist 2 Playlist Harder";

	public override string Description => "Allow to define rules to generate/update playlists.";

	public Plugin(IApplicationPaths applicationPaths,
				  IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer) =>
			Instance = this;

	public IEnumerable<PluginPageInfo> GetPages() {
		return new[] {
				new PluginPageInfo {
						Name = "configPage.html",
						EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html",
				}
		};
	}
}
