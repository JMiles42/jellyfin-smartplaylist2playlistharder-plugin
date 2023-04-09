namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

public class Operand {

	public List<string> Actors { get; set; } = new();

	public List<string> Composers { get; set; } = new();

	public List<string> Directors { get; set; } = new();

	public List<string> Genres { get; set; } = new();

	public List<string> GuestStars { get; set; } = new();

	public List<string> Producers { get; set; } = new();

	public List<string> Studios { get; set; } = new();

	public List<string> Writers { get; set; } = new();

	public List<string> Artists { get; set; } = new();

	public float CommunityRating { get; set; } = 0;

	public float CriticRating { get; set; } = 0;

	public bool IsPlayed { get; set; } = false;
	public bool IsFavoriteOrLiked { get; set; } = false;

	public string Name { get; set; }

	public string FolderPath { get; set; } = "";

	public double PremiereDate { get; set; } = 0;

	public string MediaType { get; set; } = "";

	public string Album { get; set; } = "";

	public double DateCreated { get; set; } = 0;

	public double DateLastRefreshed { get; set; } = 0;

	public double DateLastSaved { get; set; } = 0;

	public double DateModified { get; set; } = 0;

	public int? ProductionYear { get; set; } = null;

	public string OriginalTitle { get; set; } = "";

	public int Height { get; set; } = 0;

	public int Width { get; set; } = 0;

	public string FileNameWithoutExtension { get; set; } = "";

	public string OfficialRating { get; set; } = "";

	public bool HasSubtitles { get; set; } = false;

	public string SortName { get; set; } = "";

	public int DaysSinceCreated { get; set; } = 0;

	public int DaysSinceLastSaved { get; set; } = 0;

	public int DaysSinceLastModified { get; set; } = 0;

	public int DaysSinceLastRefreshed { get; set; } = 0;

	public int DaysSincePremiereDate { get; set; } = 0;

	public bool IsSquare => Height == Width;

	public bool IsHorizontal => Height < Width;

	public bool IsVertical => !IsHorizontal;

	public string Overview { get; set; } = "";

	public string CollectionName { get; set; } = "";

	public double? LastPlayedDate { get; set; } = null;

	public int PlayedCount { get; set; } = 0;

	public double PlayedPercentage { get; set; } = 0;

	public long PlaybackPositionTicks { get; set; } = 0;

	public Operand(string name) {
		Name = name;
	}
}
