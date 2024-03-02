using System.Text;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.SmartPlaylist.Extensions;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

#pragma warning disable CA1822 // Mark members as static
public class Operand
{
	public BaseItem BaseItem { get; }

	private readonly ILibraryManager  _libraryManager;

	public Operand(ILibraryManager libraryManager, BaseItem baseItem, IUserDataManager userDataManager, User user) {
		BaseItem         = baseItem;
		_libraryManager  = libraryManager;
		Name             = baseItem.Name.ValueOrEmpty();

		CommunityRating          = baseItem.CommunityRating.GetValueOrDefault();
		CriticRating             = baseItem.CriticRating.GetValueOrDefault();
		MediaType                = baseItem.MediaType.ValueOrEmpty();
		Album                    = baseItem.Album.ValueOrEmpty();
		FolderPath               = baseItem.ContainingFolderPath.ValueOrEmpty();
		ProductionYear           = baseItem.ProductionYear;
		OriginalTitle            = baseItem.OriginalTitle.ValueOrEmpty();
		Height                   = baseItem.Height;
		Width                    = baseItem.Width;
		FileNameWithoutExtension = baseItem.FileNameWithoutExtension.ValueOrEmpty();
		OfficialRating           = baseItem.OfficialRating.ValueOrEmpty();
		SortName                 = baseItem.SortName.ValueOrEmpty();
		DaysSinceCreated         = GetDaysAgo(baseItem.DateCreated);
		DaysSinceLastRefreshed   = GetDaysAgo(baseItem.DateLastRefreshed);
		DaysSinceLastSaved       = GetDaysAgo(baseItem.DateLastSaved);
		DaysSinceLastModified    = GetDaysAgo(baseItem.DateModified);
		Overview                 = baseItem.Overview.ValueOrEmpty();
		Container                = baseItem.Container.ValueOrEmpty();
		ParentName               = baseItem.LatestItemsIndexContainer?.Name.ValueOrEmpty();
		GrandparentName          = baseItem.LatestItemsIndexContainer?.DisplayParent?.Name.ValueOrEmpty();

		if (baseItem.PremiereDate.HasValue) {
			DaysSincePremiereDate = GetDaysAgo(baseItem.PremiereDate.Value, DateTime.Now);
			PremiereDate          = GetUnixSeconds(baseItem.PremiereDate.Value);
		}

		DateCreated       = GetUnixSeconds(baseItem.DateCreated);
		DateLastSaved     = GetUnixSeconds(baseItem.DateLastSaved);
		DateModified      = GetUnixSeconds(baseItem.DateModified);
		DateLastRefreshed = GetUnixSeconds(baseItem.DateLastRefreshed);

		switch (baseItem) {
			case MediaBrowser.Controller.Entities.Movies.Movie movie:
				HasSubtitles   = movie.HasSubtitles;
				CollectionName = movie.CollectionName.ValueOrEmpty();
				LengthTicks    = movie.RunTimeTicks ?? 0;

				break;
			case MediaBrowser.Controller.Entities.TV.Episode episode:
				HasSubtitles      = episode.HasSubtitles;
				AiredSeasonNumber = episode.AiredSeasonNumber;
				ParentIndexNumber = episode.ParentIndexNumber;
				SeasonName        = episode.SeasonName.ValueOrEmpty();
				SeriesName        = episode.SeriesName.ValueOrEmpty();
				LengthTicks       = episode.RunTimeTicks ?? 0;

				break;
			case MusicVideo musicVideo:
				HasSubtitles = musicVideo.HasSubtitles;
				LengthTicks  = musicVideo.RunTimeTicks ?? 0;

				break;
		}

		var userData = userDataManager.GetUserData(user, baseItem);

		if (userData is not null) {
			IsPlayed    = userData.Played;
			PlayedCount = userData.PlayCount;

			if (userData.LastPlayedDate.HasValue) {
				LastPlayedDate = GetUnixSeconds(userData.LastPlayedDate.Value);
			}

			IsFavoriteOrLiked     = userData.IsFavorite;
			PlaybackPositionTicks = userData.PlaybackPositionTicks;



			if (!baseItem.RunTimeTicks.HasValue) {
				return;
			}

			double pct = baseItem.RunTimeTicks.Value;

			if (pct <= 0) {
				return;
			}

			pct = userData.PlaybackPositionTicks / pct;

			if (pct > 0) {
				PlayedPercentage = 100 * pct;
			}
		}
	}

	private static double GetUnixSeconds(DateTime datetime) {
		try {
			return new DateTimeOffset(datetime).ToUnixTimeSeconds();
		}
		catch {
			//Ignore
		}

		return 0;
	}

	public static int GetDaysAgo(DateTime currentDate) => GetDaysAgo(currentDate, DateTime.Now);

	public static int GetDaysAgo(DateTime currentDate, DateTime now) => (int)(now - currentDate).TotalDays;

	public List<string> Actors {
		get {
			if (_actors is not null) {
				return _actors;
			}

			var people = GetPeople();

			_actors = new(people.Where(x => x.Type.Equals("Actor", StringComparison.OrdinalIgnoreCase))
								.Select(x => x.Name));

			return _actors;
		}
	}

	public List<string> Artists {
		get {
			if (_artists is not null) {
				return _artists;
			}

			_artists = new();


			if (BaseItem is MusicVideo mv) {
				_artists.AddRange(mv.Artists);
			}

			return _artists;
		}
	}

	public List<string> Composers {
		get {
			if (_composers is not null) {
				return _composers;
			}

			var people = GetPeople();

			_composers = new(people.Where(x => x.Type.Equals("Composer", StringComparison.OrdinalIgnoreCase))
								   .Select(x => x.Name));

			return _composers;
		}
	}

	public List<string> Directors {
		get {
			if (_directors is not null) {
				return _directors;
			}

			var people = GetPeople();

			_directors = new(people.Where(x => x.Type.Equals("Director", StringComparison.OrdinalIgnoreCase))
								   .Select(x => x.Name));

			return _directors;
		}
	}

	public List<string> GuestStars {
		get {
			if (_guestStars is not null) {
				return _guestStars;
			}

			var people = GetPeople();

			_guestStars = new(people.Where(x => x.Type.Equals("GuestStar", StringComparison.OrdinalIgnoreCase))
									.Select(x => x.Name));

			return _guestStars;
		}
	}

	public List<string> Producers {
		get {
			if (_producers is not null) {
				return _producers;
			}

			var people = GetPeople();

			_producers = new(people.Where(x => x.Type.Equals("Producer", StringComparison.OrdinalIgnoreCase))
								   .Select(x => x.Name));

			return _producers;
		}
	}

	public List<string> Writers {
		get {
			if (_writers is not null) {
				return _writers;
			}

			var people = GetPeople();

			_writers = new(people.Where(x => x.Type.Equals("Writer", StringComparison.OrdinalIgnoreCase))
								 .Select(x => x.Name));

			return _writers;
		}
	}

	public List<string> Genres {
		get {
			if (_genres is not null) {
				return _genres;
			}

			_genres = new();
			_genres.AddRange(BaseItem.Genres);

			return _genres;
		}
	}

	public List<string> Studios {
		get {
			if (_studios is not null) {
				return _studios;
			}

			_studios = new();
			_studios.AddRange(BaseItem.Studios);

			return _studios;
		}
	}

	public List<string> Tags {
		get {
			if (_tags is not null) {
				return _tags;
			}

			_tags = new();
			_tags.AddRange(BaseItem.Tags);

			return _tags;
		}
	}

	public List<string> PathSegment {
		get {
			if (_pathSegment is not null) {
				return _pathSegment;
			}

			_pathSegment = new();
			_pathSegment.AddRange(BaseItem.Path.Split('\\','/', StringSplitOptions.RemoveEmptyEntries));

			return _pathSegment;
		}
	}

	private List<string>? _actors;
	private List<string>? _artists;
	private List<string>? _composers;
	private List<string>? _directors;
	private List<string>? _guestStars;
	private List<string>? _producers;
	private List<string>? _writers;
	private List<string>? _genres;
	private List<string>? _studios;
	private List<string>? _tags;
	private List<string>? _pathSegment;

	private List<PersonInfo>? _people;

	private List<PersonInfo> GetPeople() {
		if (_people is not null) {
			return _people;
		}

		return _people = _libraryManager.GetPeople(BaseItem);
	}

	public bool Exists => true;

	public bool HasSubtitles { get; }

	public bool IsFavoriteOrLiked { get; }

	public bool IsHorizontal => Height < Width;

	public bool IsPlayed { get; }

	public bool IsSquare => Height == Width;

	public bool IsVertical => !IsHorizontal;

	public double DateCreated { get; }

	public double DateLastRefreshed { get; }

	public double DateLastSaved { get; }

	public double DateModified { get; }

	public double PlayedPercentage { get; }

	public double PremiereDate { get; }

	public double? LastPlayedDate { get; }

	public float CommunityRating { get; }

	public float CriticRating { get; }

	public int DaysSinceCreated { get; }

	public int DaysSinceLastModified { get; }

	public int DaysSinceLastRefreshed { get; }

	public int DaysSinceLastSaved { get; }

	public int DaysSincePremiereDate { get; }

	public int Height { get; }

	public int PlayedCount { get; }

	public int Width { get; }

	public int? AiredSeasonNumber { get; }

	public int? ParentIndexNumber { get; }

	public int? ProductionYear { get; }

	public long PlaybackPositionTicks { get; }

	public string? Album { get; }

	public string? CollectionName { get; }

	public string? Container { get; }

	public string? FileNameWithoutExtension { get; }

	public string? FolderPath { get; }

	public string? GrandparentName { get; }

	public string? MediaType { get; }

	public string? Name { get; }

	public string? OfficialRating { get; }

	public string? OriginalTitle { get; }

	public string? Overview { get; }

	public string? ParentName { get; }

	public string? SeasonName { get; }

	public string? SeriesName { get; }

	public string? SortName { get; }

	public TimeSpan Length => TimeSpan.FromTicks(LengthTicks);

	public double LengthSeconds => Length.TotalSeconds;

	public double LengthMinutes => Length.TotalMinutes;

	public double LengthHours => Length.TotalHours;

	public long LengthTicks { get; }

	public string AllText {
		get {
			if (string.IsNullOrEmpty(_allText)) {
				_allText = GetAllText();
			}

			return _allText;
		}
	}

	private string? _allText;

	private string GetAllText() {
		var sb = new StringBuilder();

		Actors.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));

		foreach (var a in Artists) {
			sb.AppendLineIfNotNullOrEmpty(a);
		}

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
