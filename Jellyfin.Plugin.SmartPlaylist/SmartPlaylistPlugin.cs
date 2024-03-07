namespace Jellyfin.Plugin.SmartPlaylist;

public class SmartPlaylistPlugin: BasePlugin<SmartPlaylistPluginConfiguration>, IHasWebPages
{
	public static SmartPlaylistPlugin? Instance { get; private set; }

	public override Guid Id => Guid.Parse(SmartPlaylistConsts.PLUGIN_GUID);

	public override string Name => SmartPlaylistConsts.PLUGIN_NAME;

	public override string Description => SmartPlaylistConsts.PLUGIN_DESCRIPTION;

	public SmartPlaylistPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer):
			base(applicationPaths, xmlSerializer) =>
			Instance = this;

	public IEnumerable<PluginPageInfo> GetPages() =>
			new[]
			{
				new PluginPageInfo
				{
					Name = "configPage.html",
					EmbeddedResourcePath = $"{GetType().Namespace}.Pages.configPage.html",
				},
				new PluginPageInfo
				{
					DisplayName          = SmartPlaylistConsts.PLUGIN_NAME,
					EnableInMainMenu     = true,
					EmbeddedResourcePath = $"{GetType().Namespace}.Pages.pluginDataPage.html",
					Name                 = "pluginDataPage.html",
					MenuIcon             = "playlist_add_check_circle",
				},
			};
}
