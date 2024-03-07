using Jellyfin.Plugin.SmartPlaylist.Interfaces;

namespace Jellyfin.Plugin.SmartPlaylist.Configuration;

public class SmartPlaylistPluginConfiguration: BasePluginConfiguration, ISmartPlaylistPluginConfiguration {
	public int PlaylistSorterThreadCount { get; set; } = 5;

	public bool PlaylistDetailedErrors { get; set; } = false;

	public bool AlwaysSaveFile { get; set; } = false;

	public bool BackupFileOnSave { get; set; } = false;

	internal const string PLAYLIST_FOLDER_NAME = "smartplaylists";
	private       string playlistFolderName   = PLAYLIST_FOLDER_NAME;
	public string PlaylistFolderName
	{
		get => playlistFolderName;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				playlistFolderName = PLAYLIST_FOLDER_NAME;
			}
			else
			{
				playlistFolderName = value;
			}
		}
	}

	internal const string PLAYLIST_BACKUP_FOLDER_NAME = "smartplaylists_backups";
	private string playlistBackupFolderName = PLAYLIST_BACKUP_FOLDER_NAME;

	public string PlaylistBackupFolderName
	{
		get => playlistBackupFolderName;
		set {
			if (string.IsNullOrWhiteSpace(value)) {
				playlistBackupFolderName = PLAYLIST_BACKUP_FOLDER_NAME;
			}
			else {
				playlistBackupFolderName = value;
			}
		}
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="SmartPlaylistPluginConfiguration" /> class.
	/// </summary>
	public SmartPlaylistPluginConfiguration() { }
}
