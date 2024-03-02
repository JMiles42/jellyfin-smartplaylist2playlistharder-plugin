using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.SmartPlaylist.Api;

/// <summary>
/// Controller for accessing SmartPlaylist data.
/// </summary>
[ApiController]
[Route("SmartPlaylist")]
public class SmartPlaylistController: ControllerBase
{
	[Authorize(Policy = "DefaultAuthorization")]
	[HttpGet(nameof(GetAllPlaylistRunDetails))]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[Produces("application/javascript")]
	public IActionResult GetAllPlaylistRunDetails() {
		var all = SmartPlaylistManager.GetAllRunDetails()
									  .OrderBy(a => a, SmartPlaylistLastRunDetailsComparer.Instance)
									  .Select(GetResultObject);

		return new JsonResult(all);
	}

	public record SmartPlaylistLastRunDetailsDto(string PlaylistId, string Status, IEnumerable<string>? Errors = null, Guid? JellyfinPlaylistId = null);

	private static SmartPlaylistLastRunDetailsDto GetResultObject(SmartPlaylistLastRunDetails runDetails) {
		if (runDetails.Errors is null) {
			return new(runDetails.PlaylistId, runDetails.Status, JellyfinPlaylistId: runDetails.JellyfinPlaylistId);
		}

		return new(runDetails.PlaylistId, runDetails.Status, runDetails.Errors?.Select(Selector), runDetails.JellyfinPlaylistId);
	}

	private static string Selector(SmartPlaylistsRefreshError e) {
		if (e.Exception is null) {
			return e.ErrorPrefix;
		}

		if (SmartPlaylistPlugin.Instance?.Configuration.PlaylistDetailedErrors is true) {
			return e.ErrorPrefix + " " + e.Exception;
		}

		return e.ErrorPrefix + " " + e.Exception?.Message;
	}

}