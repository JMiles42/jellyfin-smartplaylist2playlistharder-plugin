using Jellyfin.Plugin.SmartPlaylist.Api.Models;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Jellyfin.Plugin.SmartPlaylist.Api;

/// <summary>
///     Controller for accessing SmartPlaylist data.
/// </summary>
[ApiController]
[Route("SmartPlaylist")]
public class SmartPlaylistController: ControllerBase
{
	private readonly ILibraryManager                  _libraryManager;
	private readonly OperandFactory                   _operandFactory;
	private readonly IUserManager                     _userManager;
	private readonly ILogger<SmartPlaylistController> _logger;

	public SmartPlaylistController(ILibraryManager libraryManager,
								   OperandFactory  operandFactory,
								   IUserManager userManager,
								   ILogger<SmartPlaylistController> logger)
	{
		_libraryManager = libraryManager;
		_operandFactory = operandFactory;
		_userManager    = userManager;
		_logger     = logger;
	}


	[Authorize(Policy = "DefaultAuthorization")]
	[HttpGet("[Action]")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[Produces("application/javascript")]
	public IActionResult GetAllPlaylistRunDetails() {
		var all = SmartPlaylistManager.GetAllRunDetails()
									  .OrderBy(a => a,
											   SmartPlaylistLastRunDetailsComparer.Instance)
									  .Select(GetResultObject);

		return new JsonResult(all);
	}

	////TODO: Test this works
	//[Authorize(Policy = "DefaultAuthorization")]
	//[HttpGet("[Action]/{userId}/{itemId}")]
	//[ProducesResponseType(StatusCodes.Status200OK)]
	//[Produces("application/javascript")]
	//public IActionResult GetOperandFromItemIdAndUserId([FromRoute] Guid userId, [FromRoute] Guid itemId) {
	//	try {
	//		var baseItem = _libraryManager.GetItemById(itemId);
	//		var user     = _userManager.GetUserById(userId);
	//
	//		if (baseItem is null || user is null) {
	//			return new JsonResult(new { Error = new { Message = "No user or baseItem found", }, });
	//		}
	//
	//		return new JsonResult(new { Item = _operandFactory.Create(baseItem, user), });
	//	}
	//	catch (Exception exception) {
	//		return new JsonResult(new { Error = exception, });
	//	}
	//}

	//[Authorize(Policy = "DefaultAuthorization")]
	//[HttpGet("[Action]/{userName}/{itemId}")]
	//[ProducesResponseType(StatusCodes.Status200OK)]
	//[Produces("application/javascript")]
	//public IActionResult GetOperandFromItemIdAndUserName([FromRoute] string userName, [FromRoute] Guid itemId) {
	//	try {
	//		var baseItem = _libraryManager.GetItemById(itemId);
	//		var user     = _userManager.GetUserByName(userName);
	//
	//		if (baseItem is null || user is null) {
	//			return new JsonResult(new { Error = new { Message = "No user or baseItem found", }, });
	//		}
	//
	//		return new JsonResult(new { Item = _operandFactory.Create(baseItem, user), });
	//	}
	//	catch (Exception exception) {
	//		return new JsonResult(new { Error = exception, });
	//	}
	//}

	private static SmartPlaylistLastRunDetailsDto GetResultObject(SmartPlaylistLastRunDetails runDetails)
	{
		if (runDetails.Errors is null)
		{
			return new(runDetails.PlaylistId, runDetails.Status, JellyfinPlaylistId: runDetails.JellyfinPlaylistId);
		}

		return new(runDetails.PlaylistId,
				   runDetails.Status,
				   runDetails.Errors?.Select(Selector),
				   runDetails.JellyfinPlaylistId);
	}

	private static string Selector(SmartPlaylistsRefreshError e)
	{
		if (e.Exception is null)
		{
			return e.ErrorPrefix;
		}

		if (SmartPlaylistPlugin.Instance?.Configuration.PlaylistDetailedErrors is true)
		{
			return e.ErrorPrefix + " " + e.Exception;
		}

		return e.ErrorPrefix + " " + e.Exception?.Message;
	}
}