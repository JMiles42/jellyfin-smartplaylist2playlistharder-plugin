using Jellyfin.Data.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

internal class OperandFactory {
	// Returns a specific operand provided a baseitem, user, and library manager object.
	public static Operand GetMediaType(ILibraryManager libraryManager,
									   BaseItem baseItem,
									   User user,
									   IUserDataManager userDataManager = null) {
		userDataManager ??= BaseItem.UserDataManager;

		var operand = new Operand(baseItem.Name);
		ProcessPeople(libraryManager, baseItem, operand);

		operand.Genres.AddRange(baseItem.Genres);
		operand.Studios.AddRange(baseItem.Studios);
		operand.CommunityRating = baseItem.CommunityRating.GetValueOrDefault();
		operand.CriticRating = baseItem.CriticRating.GetValueOrDefault();
		operand.MediaType = baseItem.MediaType;
		operand.Album = baseItem.Album;
		operand.FolderPath = baseItem.ContainingFolderPath;
		operand.ProductionYear = baseItem.ProductionYear;
		operand.OriginalTitle = baseItem.OriginalTitle;
		operand.Height = baseItem.Height;
		operand.Width = baseItem.Width;
		operand.FileNameWithoutExtension = baseItem.FileNameWithoutExtension;
		operand.OfficialRating = baseItem.OfficialRating;
		operand.SortName = baseItem.SortName;
		operand.DaysSinceCreated = GetDaysAgo(baseItem.DateCreated);
		operand.DaysSinceLastRefreshed = GetDaysAgo(baseItem.DateLastRefreshed);
		operand.DaysSinceLastSaved = GetDaysAgo(baseItem.DateLastSaved);
		operand.DaysSinceLastModified = GetDaysAgo(baseItem.DateModified);
		operand.Overview = baseItem.Overview;

		ProcessPlayedData(userDataManager, operand, baseItem, user);

		ProcessTypes(baseItem, operand);

		if (baseItem.PremiereDate.HasValue) {
			operand.DaysSincePremiereDate = GetDaysAgo(baseItem.PremiereDate.Value, DateTime.Now);
			operand.PremiereDate = GetUnixSeconds(baseItem.PremiereDate.Value);
		}

		operand.DateCreated = GetUnixSeconds(baseItem.DateCreated);
		operand.DateLastRefreshed = GetUnixSeconds(baseItem.DateLastRefreshed);
		operand.DateLastSaved = GetUnixSeconds(baseItem.DateLastSaved);
		operand.DateModified = GetUnixSeconds(baseItem.DateModified);

		return operand;
	}

	private static void ProcessPlayedData(IUserDataManager userDataManager, Operand operand, BaseItem baseItem, User user) {
		var data = userDataManager.GetUserData(user, baseItem);
		operand.IsPlayed = data.Played;
		operand.PlayedCount = data.PlayCount;

		if (data.LastPlayedDate.HasValue) {
			operand.LastPlayedDate = GetUnixSeconds(data.LastPlayedDate.Value);
		}

		operand.IsFavoriteOrLiked = data.IsFavorite;
		operand.PlaybackPositionTicks = data.PlaybackPositionTicks;

		ProcessRuntime(operand, baseItem, data);
	}

	private static void ProcessRuntime(Operand operand, BaseItem baseItem, UserItemData data) {
		if (!baseItem.RunTimeTicks.HasValue) {
			return;
		}

		double pct = baseItem.RunTimeTicks.Value;

		if (pct <= 0) {
			return;
		}

		pct = data.PlaybackPositionTicks / pct;

		if (pct > 0) {
			operand.PlayedPercentage = 100 * pct;
		}
	}

	private static void ProcessPeople(ILibraryManager libraryManager, BaseItem baseItem, Operand operand) {
		var people = libraryManager.GetPeople(baseItem);

		if (!people.Any()) {
			return;
		}

		operand.Actors.AddRange(people.Where(x => x.Type.Equals("Actor")).Select(x => x.Name));
		operand.Composers.AddRange(people.Where(x => x.Type.Equals("Composer")).Select(x => x.Name));
		operand.Directors.AddRange(people.Where(x => x.Type.Equals("Director")).Select(x => x.Name));
		operand.GuestStars.AddRange(people.Where(x => x.Type.Equals("GuestStar")).Select(x => x.Name));
		operand.Producers.AddRange(people.Where(x => x.Type.Equals("Producer")).Select(x => x.Name));
		operand.Writers.AddRange(people.Where(x => x.Type.Equals("Writer")).Select(x => x.Name));
	}

	private static void ProcessTypes(BaseItem baseItem, Operand operand) {
		switch (baseItem) {
			case Movie movie:
				operand.HasSubtitles = movie.HasSubtitles;
				operand.CollectionName = movie.CollectionName;

				break;
			case Episode episode:
				operand.HasSubtitles = episode.HasSubtitles;

				break;
			case MusicVideo musicVideo:
				operand.HasSubtitles = musicVideo.HasSubtitles;
				operand.Artists.AddRange(musicVideo.Artists);

				break;
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
}
