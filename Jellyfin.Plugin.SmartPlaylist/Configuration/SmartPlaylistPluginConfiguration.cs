namespace Jellyfin.Plugin.SmartPlaylist.Configuration;

/// <summary>
///     Class PluginConfiguration
/// </summary>
public class SmartPlaylistPluginConfiguration: BasePluginConfiguration
{

	public int PlaylistSorterThreadCount { get; set; } = 10;

	public bool PlaylistDetailedErrors { get; set; } = false;

	/// <summary>
	///     Initializes a new instance of the <see cref="SmartPlaylistPluginConfiguration" /> class.
	/// </summary>
	public SmartPlaylistPluginConfiguration() { }
}
