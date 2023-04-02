using Jellyfin.Data.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine;

internal class OperandFactory {
	// Returns a specific operand provided a baseitem, user, and library manager object.
	public static Operand GetMediaType(ILibraryManager libraryManager,
									   BaseItem baseItem,
									   User user,
									   ILogger logger = null) {
		var operand = new Operand(baseItem.Name);
		var people = libraryManager.GetPeople(baseItem);

		if (people.Any()) {
			// Maps to MediaBrowser.Model.Entities.PersonType
			operand.Actors = people.Where(x => x.Type.Equals("Actor")).Select(x => x.Name).ToList();
			operand.Composers = people.Where(x => x.Type.Equals("Composer")).Select(x => x.Name).ToList();
			operand.Directors = people.Where(x => x.Type.Equals("Director")).Select(x => x.Name).ToList();
			operand.GuestStars = people.Where(x => x.Type.Equals("GuestStar")).Select(x => x.Name).ToList();
			operand.Producers = people.Where(x => x.Type.Equals("Producer")).Select(x => x.Name).ToList();
			operand.Writers = people.Where(x => x.Type.Equals("Writer")).Select(x => x.Name).ToList();
		}

		operand.Genres = baseItem.Genres.ToList();
		operand.IsPlayed = baseItem.IsPlayed(user);
		operand.Studios = baseItem.Studios.ToList();
		operand.CommunityRating = baseItem.CommunityRating.GetValueOrDefault();
		operand.CriticRating = baseItem.CriticRating.GetValueOrDefault();
		operand.MediaType = baseItem.MediaType;
		operand.Album = baseItem.Album;

		try {
			//logger?.LogInformation("{Name} PremiereDate: {PremiereDate}", baseItem.Name, baseItem.PremiereDate);
			if (baseItem.PremiereDate.HasValue)
				operand.PremiereDate = new DateTimeOffset(baseItem.PremiereDate.Value).ToUnixTimeSeconds();

			//logger?.LogInformation("{Name} DateCreated: {DateCreated}", baseItem.Name, baseItem.PremiereDate);
			operand.DateCreated = new DateTimeOffset(baseItem.DateCreated).ToUnixTimeSeconds();
			//logger?.LogInformation("{Name} DateLastRefreshed: {DateLastRefreshed}", baseItem.Name, baseItem.PremiereDate);
			operand.DateLastRefreshed = new DateTimeOffset(baseItem.DateLastRefreshed).ToUnixTimeSeconds();
			//logger?.LogInformation("{Name} DateLastSaved: {DateLastSaved}", baseItem.Name, baseItem.PremiereDate);
			operand.DateLastSaved = new DateTimeOffset(baseItem.DateLastSaved).ToUnixTimeSeconds();
			//logger?.LogInformation("{Name} DateModified: {DateModified}", baseItem.Name, baseItem.PremiereDate);
			operand.DateModified = new DateTimeOffset(baseItem.DateModified).ToUnixTimeSeconds();
		}
		catch {
			//Ignore
		}

		operand.FolderPath = baseItem.ContainingFolderPath;

		return operand;
	}
}
