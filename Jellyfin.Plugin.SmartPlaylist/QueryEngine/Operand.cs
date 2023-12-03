using System.Text;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

#pragma warning disable CA1822 // Mark members as static
public class Operand {

	public List<string> Actors                   { get; set; } = new();
	public List<string> Artists                  { get; set; } = new();
	public List<string> Composers                { get; set; } = new();
	public List<string> Directors                { get; set; } = new();
	public List<string> Genres                   { get; set; } = new();
	public List<string> GuestStars               { get; set; } = new();
	public List<string> Producers                { get; set; } = new();
	public List<string> Studios                  { get; set; } = new();
	public List<string> Writers                  { get; set; } = new();
	public List<string> Tags                     { get; set; } = new();
	public bool         Exists                   => true;
	public bool         HasSubtitles             { get; set; } = false;
	public bool         IsFavoriteOrLiked        { get; set; } = false;
	public bool         IsHorizontal             => Height < Width;
	public bool         IsPlayed                 { get; set; } = false;
	public bool         IsSquare                 => Height == Width;
	public bool         IsVertical               => !IsHorizontal;
	public double       DateCreated              { get; set; } = 0;
	public double       DateLastRefreshed        { get; set; } = 0;
	public double       DateLastSaved            { get; set; } = 0;
	public double       DateModified             { get; set; } = 0;
	public double       PlayedPercentage         { get; set; } = 0;
	public double       PremiereDate             { get; set; } = 0;
	public double?      LastPlayedDate           { get; set; } = null;
	public float        CommunityRating          { get; set; } = 0;
	public float        CriticRating             { get; set; } = 0;
	public int          DaysSinceCreated         { get; set; } = 0;
	public int          DaysSinceLastModified    { get; set; } = 0;
	public int          DaysSinceLastRefreshed   { get; set; } = 0;
	public int          DaysSinceLastSaved       { get; set; } = 0;
	public int          DaysSincePremiereDate    { get; set; } = 0;
	public int          Height                   { get; set; } = 0;
	public int          PlayedCount              { get; set; } = 0;
	public int          Width                    { get; set; } = 0;
	public int?         AiredSeasonNumber        { get; set; } = null;
	public int?         ParentIndexNumber        { get; set; } = null;
	public int?         ProductionYear           { get; set; } = null;
	public long         PlaybackPositionTicks    { get; set; } = 0;
	public string       Album                    { get; set; } = string.Empty;
	public string       CollectionName           { get; set; } = string.Empty;
	public string       Container                { get; set; } = string.Empty;
	public string       FileNameWithoutExtension { get; set; } = string.Empty;
	public string       FolderPath               { get; set; } = string.Empty;
	public string       GrandparentName          { get; set; } = string.Empty;
	public string       MediaType                { get; set; } = string.Empty;
	public string       Name                     { get; set; }
	public string       OfficialRating           { get; set; } = string.Empty;
	public string       OriginalTitle            { get; set; } = string.Empty;
	public string       Overview                 { get; set; } = string.Empty;
	public string       ParentName               { get; set; } = string.Empty;
	public string       SeasonName               { get; set; } = string.Empty;
	public string       SeriesName               { get; set; } = string.Empty;
	public string       SortName                 { get; set; } = string.Empty;
	public TimeSpan     Length                   => TimeSpan.FromTicks(LengthTicks);
	public double       LengthSeconds            => Length.TotalSeconds;
	public double       LengthMinutes            => Length.TotalMinutes;
	public double       LengthHours              => Length.TotalHours;
	public long         LengthTicks              { get; set; } = 0;

	public string AllText {
		get {
			if (string.IsNullOrEmpty(_allText)) {
				_allText = GetAllText();
			}
			return _allText;
		}
	}

	private string _allText;

	public Operand(string name) {
		Name = name;
	}

	private string GetAllText() {
		var sb = new StringBuilder();

		Actors.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		Artists.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		Composers.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		Directors.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		GuestStars.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		Producers.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		Studios.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		Writers.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		sb.AppendLineIfNotNullOrEmpty(Album);
		sb.AppendLineIfNotNullOrEmpty(CollectionName);
		sb.AppendLineIfNotNullOrEmpty(Container);
		sb.AppendLineIfNotNullOrEmpty(FileNameWithoutExtension);
		sb.AppendLineIfNotNullOrEmpty(FolderPath);
		sb.AppendLineIfNotNullOrEmpty(GrandparentName);
		sb.AppendLineIfNotNullOrEmpty(MediaType);
		sb.AppendLineIfNotNullOrEmpty(Name);
		sb.AppendLineIfNotNullOrEmpty(OfficialRating);
		sb.AppendLineIfNotNullOrEmpty(OriginalTitle);
		sb.AppendLineIfNotNullOrEmpty(Overview);
		sb.AppendLineIfNotNullOrEmpty(ParentName);
		sb.AppendLineIfNotNullOrEmpty(SeasonName);
		sb.AppendLineIfNotNullOrEmpty(SeriesName);
		sb.AppendLineIfNotNullOrEmpty(SortName);
		return sb.ToString();
	}
}
#pragma warning restore CA1822 // Mark members as static
