namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

#pragma warning disable CA1822 // Mark members as static
public record Operand
{
	private readonly ILibraryManager _libraryManager;
	private readonly object          _locker = new();

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

	private string? _allText;

	public BaseItem BaseItem { get; }

	public List<string> Actors
	{
		get
		{
			lock (_locker)
			{
				if (_actors is not null)
				{
					return _actors;
				}

				var people = GetPeople();

				_actors = new(people.Where(x => x.Type.Equals("Actor", StringComparison.OrdinalIgnoreCase))
									.Select(x => x.Name));

				return _actors;
			}
		}
	}

	public List<string> Artists
	{
		get
		{
			lock (_locker)
			{
				if (_artists is not null)
				{
					return _artists;
				}


				_artists = new();

				if (BaseItem is MusicVideo mv)
				{
					_artists.AddRange(mv.Artists);
				}

				return _artists;
			}
		}
	}

	public List<string> Composers
	{
		get
		{
			lock (_locker)
			{
				if (_composers is not null)
				{
					return _composers;
				}

				var people = GetPeople();

				_composers = new(people.Where(x => x.Type.Equals("Composer", StringComparison.OrdinalIgnoreCase))
									   .Select(x => x.Name));

				return _composers;
			}
		}
	}

	public List<string> Directors
	{
		get
		{
			lock (_locker)
			{
				if (_directors is not null)
				{
					return _directors;
				}

				var people = GetPeople();

				_directors = new(people.Where(x => x.Type.Equals("Director", StringComparison.OrdinalIgnoreCase))
									   .Select(x => x.Name));

				return _directors;
			}
		}
	}

	public List<string> GuestStars
	{
		get
		{
			lock (_locker)
			{
				if (_guestStars is not null)
				{
					return _guestStars;
				}

				var people = GetPeople();

				_guestStars = new(people.Where(x => x.Type.Equals("GuestStar", StringComparison.OrdinalIgnoreCase))
										.Select(x => x.Name));

				return _guestStars;
			}
		}
	}

	public List<string> Producers
	{
		get
		{
			lock (_locker)
			{
				if (_producers is not null)
				{
					return _producers;
				}

				var people = GetPeople();

				_producers = new(people.Where(x => x.Type.Equals("Producer", StringComparison.OrdinalIgnoreCase))
									   .Select(x => x.Name));

				return _producers;
			}
		}
	}

	public List<string> Writers
	{
		get
		{
			lock (_locker)
			{
				if (_writers is not null)
				{
					return _writers;
				}

				var people = GetPeople();

				_writers = new(people.Where(x => x.Type.Equals("Writer", StringComparison.OrdinalIgnoreCase))
									 .Select(x => x.Name));

				return _writers;
			}
		}
	}

	public List<string> Genres
	{
		get
		{
			lock (_locker)
			{
				if (_genres is not null)
				{
					return _genres;
				}

				_genres = new();
				_genres.AddRange(BaseItem.Genres);

				return _genres;
			}
		}
	}

	public List<string> Studios
	{
		get
		{
			lock (_locker)
			{
				if (_studios is not null)
				{
					return _studios;
				}

				_studios = new();
				_studios.AddRange(BaseItem.Studios);

				return _studios;
			}
		}
	}

	public List<string> Tags
	{
		get
		{
			lock (_locker)
			{
				if (_tags is not null)
				{
					return _tags;
				}

				_tags = new();
				_tags.AddRange(BaseItem.Tags);

				return _tags;
			}
		}
	}

	public List<string> PathSegment
	{
		get
		{
			lock (_locker)
			{
				if (_pathSegment is not null)
				{
					return _pathSegment;
				}

				_pathSegment = new();
				_pathSegment.AddRange(BaseItem.Path.Split('\\', '/', StringSplitOptions.RemoveEmptyEntries));

				return _pathSegment;
			}
		}
	}

	public int ActorsLength => Actors.Count;

	public int ArtistsLength => Artists.Count;

	public int ComposersLength => Composers.Count;

	public int DirectorsLength => Directors.Count;

	public int GuestStarsLength => GuestStars.Count;

	public int ProducersLength => Producers.Count;

	public int WritersLength => Writers.Count;

	public int GenresLength => Genres.Count;

	public int StudiosLength => Studios.Count;

	public int TagsLength => Tags.Count;

	public int PathSegmentLength => PathSegment.Count;

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

	public double LastPlayedDate { get; }

	public int DaysSinceCreated { get; }

	public int DaysSinceLastModified { get; }

	public int DaysSinceLastRefreshed { get; }

	public int DaysSinceLastSaved { get; }

	public int DaysSincePremiereDate { get; }

	public int PlayedCount { get; }

	public int? AiredSeasonNumber { get; }

	public long PlaybackPositionTicks { get; }

	public string CollectionName { get; }

	public string SeasonName { get; }

	public string SeriesName { get; }

	public string AllText
	{
		get
		{
			lock (_locker)
			{
				return _allText ??= GetAllText(this);
			}
		}
	}

	public int Height => BaseItem.Height;

	public int Width => BaseItem.Width;

	public int? ProductionYear => BaseItem.ProductionYear;

	public int? ParentIndexNumber => BaseItem.ParentIndexNumber;

	public string OriginalTitle => BaseItem.OriginalTitle;

	public string Name => BaseItem.Name;

	public string MediaType => BaseItem.MediaType;

	public string Album => BaseItem.Album;

	public string FolderPath => BaseItem.ContainingFolderPath;

	public string ContainingFolderPath => FolderPath;

	public string FileNameWithoutExtension => BaseItem.FileNameWithoutExtension;

	public string OfficialRating => BaseItem.OfficialRating;

	public string SortName => BaseItem.SortName;

	public string ForcedSortName => BaseItem.ForcedSortName;

	public string Overview => BaseItem.Overview;

	public string Container => BaseItem.Container;

	public string Path => BaseItem.Path;

	public string Tagline => BaseItem.Tagline;

	public string? ParentName => BaseItem.LatestItemsIndexContainer?.Name;

	public string? GrandparentName => BaseItem.LatestItemsIndexContainer?.DisplayParent?.Name;

	public float? CommunityRating => BaseItem.CommunityRating;

	public float? CriticRating => BaseItem.CriticRating;

	public DateTime? EndDate => BaseItem.EndDate;

	public Guid ChannelId => BaseItem.ChannelId;

	public Guid Id => BaseItem.Id;

	public TimeSpan? Length => LengthTicks.HasValue? TimeSpan.FromTicks(LengthTicks.Value) : null;

	public double? LengthSeconds => Length?.TotalSeconds;

	public double? LengthMinutes => Length?.TotalMinutes;

	public double? LengthHours => Length?.TotalHours;

	public long? LengthTicks { get; }

	public long? RunTimeTicks => LengthTicks;

	private Operand(ILibraryManager  libraryManager,
					BaseItem         baseItem,
					IUserDataManager userDataManager,
					User             user)
	{
		BaseItem        = baseItem;
		_libraryManager = libraryManager;

		DaysSinceCreated       = GetDaysAgo(baseItem.DateCreated);
		DaysSinceLastRefreshed = GetDaysAgo(baseItem.DateLastRefreshed);
		DaysSinceLastSaved     = GetDaysAgo(baseItem.DateLastSaved);
		DaysSinceLastModified  = GetDaysAgo(baseItem.DateModified);

		if (baseItem.PremiereDate.HasValue)
		{
			DaysSincePremiereDate = GetDaysAgo(baseItem.PremiereDate.Value, DateTime.Now);
			PremiereDate          = GetUnixSeconds(baseItem.PremiereDate.Value);
		}
		else
		{
			DaysSincePremiereDate = 0;
			PremiereDate          = 0;
		}

		DateCreated       = GetUnixSeconds(baseItem.DateCreated);
		DateLastSaved     = GetUnixSeconds(baseItem.DateLastSaved);
		DateModified      = GetUnixSeconds(baseItem.DateModified);
		DateLastRefreshed = GetUnixSeconds(baseItem.DateLastRefreshed);

		switch (baseItem)
		{
			case Movie movie:
				HasSubtitles      = movie.HasSubtitles;
				AiredSeasonNumber = 0;
				CollectionName    = movie.CollectionName;
				SeasonName        = string.Empty;
				SeriesName        = string.Empty;
				LengthTicks       = movie.RunTimeTicks;

				break;
			case Episode episode:
				HasSubtitles      = episode.HasSubtitles;
				AiredSeasonNumber = episode.AiredSeasonNumber;
				CollectionName    = string.Empty;
				SeasonName        = episode.SeasonName;
				SeriesName        = episode.SeriesName;
				LengthTicks       = episode.RunTimeTicks;

				break;
			case MusicVideo musicVideo:
				HasSubtitles      = musicVideo.HasSubtitles;
				AiredSeasonNumber = 0;
				CollectionName    = string.Empty;
				SeasonName        = string.Empty;
				SeriesName        = string.Empty;
				LengthTicks       = musicVideo.RunTimeTicks;

				break;
			default:
				HasSubtitles      = false;
				AiredSeasonNumber = 0;
				CollectionName    = null;
				SeasonName        = null;
				SeriesName        = null;
				LengthTicks       = null;

				break;
		}

		var userData = userDataManager.GetUserData(user, baseItem);

		if (userData is not null)
		{
			IsPlayed    = userData.Played;
			PlayedCount = userData.PlayCount;

			if (userData.LastPlayedDate.HasValue)
			{
				LastPlayedDate = GetUnixSeconds(userData.LastPlayedDate.Value);
			}

			IsFavoriteOrLiked     = userData.IsFavorite;
			PlaybackPositionTicks = userData.PlaybackPositionTicks;


			if (!baseItem.RunTimeTicks.HasValue)
			{
				return;
			}

			double pct = baseItem.RunTimeTicks.Value;

			if (pct <= 0)
			{
				return;
			}

			pct = userData.PlaybackPositionTicks / pct;

			if (pct > 0)
			{
				PlayedPercentage = 100 * pct;
			}
		}
	}

	private static double GetUnixSeconds(DateTime datetime)
	{
		try
		{
			return new DateTimeOffset(datetime).ToUnixTimeSeconds();
		}
		catch
		{
			//Ignore
		}

		return 0;
	}

	public static int GetDaysAgo(DateTime currentDate) => GetDaysAgo(currentDate, DateTime.Now);

	public static int GetDaysAgo(DateTime currentDate, DateTime now) => (int)(now - currentDate).TotalDays;

	private List<PersonInfo> GetPeople()
	{
		if (_people is not null)
		{
			return _people;
		}

		return _people = _libraryManager.GetPeople(BaseItem);
	}

	private static string GetAllText(Operand operand)
	{
		var sb = new StringBuilder();

		operand.Actors.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		operand.Artists.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		operand.Composers.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		operand.Directors.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		operand.GuestStars.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		operand.Producers.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		operand.Studios.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		operand.Writers.ForEach(a => sb.AppendLineIfNotNullOrEmpty(a));
		sb.AppendLineIfNotNullOrEmpty(operand.OriginalTitle);
		sb.AppendLineIfNotNullOrEmpty(operand.Name);
		sb.AppendLineIfNotNullOrEmpty(operand.MediaType);
		sb.AppendLineIfNotNullOrEmpty(operand.Album);
		sb.AppendLineIfNotNullOrEmpty(operand.FolderPath);
		sb.AppendLineIfNotNullOrEmpty(operand.ContainingFolderPath);
		sb.AppendLineIfNotNullOrEmpty(operand.FileNameWithoutExtension);
		sb.AppendLineIfNotNullOrEmpty(operand.OfficialRating);
		sb.AppendLineIfNotNullOrEmpty(operand.SortName);
		sb.AppendLineIfNotNullOrEmpty(operand.ForcedSortName);
		sb.AppendLineIfNotNullOrEmpty(operand.Overview);
		sb.AppendLineIfNotNullOrEmpty(operand.Container);
		sb.AppendLineIfNotNullOrEmpty(operand.Path);
		sb.AppendLineIfNotNullOrEmpty(operand.Tagline);
		sb.AppendLineIfNotNullOrEmpty(operand.ParentName);
		sb.AppendLineIfNotNullOrEmpty(operand.GrandparentName);

		return sb.ToString();
	}

	public static Operand Create(ILibraryManager  libraryManager,
								 BaseItem         baseItem,
								 IUserDataManager userDataManager,
								 User             user) =>
			new(libraryManager, baseItem, userDataManager, user);
}
#pragma warning restore CA1822 // Mark members as static
