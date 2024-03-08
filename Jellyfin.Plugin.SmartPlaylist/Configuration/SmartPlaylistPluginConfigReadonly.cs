using Jellyfin.Plugin.SmartPlaylist.Interfaces;

namespace Jellyfin.Plugin.SmartPlaylist.Configuration;

public record SmartPlaylistPluginConfigReadonly(int    PlaylistSorterThreadCount,
									 bool   PlaylistBatchedProcessing,
									 bool   PlaylistDetailedErrors,
									 bool   AlwaysSaveFile,
									 bool   BackupFileOnSave,
									 string PlaylistFolderName,
									 string PlaylistBackupFolderName): ISmartPlaylistPluginConfiguration
{
	public SmartPlaylistPluginConfigReadonly(ISmartPlaylistPluginConfiguration other): this(other.PlaylistSorterThreadCount,
		other.PlaylistBatchedProcessing,
		other.PlaylistDetailedErrors,
		other.AlwaysSaveFile,
		other.BackupFileOnSave,
		other.PlaylistFolderName,
		other.PlaylistBackupFolderName) { }
}