using System.Reflection;
using Jellyfin.Plugin.SmartPlaylist.Configuration;
using Jellyfin.Plugin.SmartPlaylist.Infrastructure;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.IO;
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
	private readonly Assembly _assembly;

	private readonly ILogger<SmartPlaylistController> _logger;
	private readonly ILibraryManager                  _libraryManager;
	private readonly IFileSystem                      _fileSystem;
	private readonly ILoggerFactory                   _loggerFactory;
	private readonly IApplicationPaths                _appPaths;
	private readonly ILibraryMonitor                  _libraryMonitor;
	private readonly IMediaEncoder                    _mediaEncoder;
	private readonly IServerConfigurationManager      _configurationManager;
	private readonly EncodingHelper                   _encodingHelper;

	private readonly SmartPlaylistPluginConfiguration _config;

	/// <summary>
	/// Initializes a new instance of the <see cref="SmartPlaylistController"/> class.
	/// </summary>
	public SmartPlaylistController(ILibraryManager                  libraryManager,
								   IFileSystem                      fileSystem,
								   ILogger<SmartPlaylistController> logger,
								   ILoggerFactory                   loggerFactory,
								   IApplicationPaths                appPaths,
								   ILibraryMonitor                  libraryMonitor,
								   IMediaEncoder                    mediaEncoder,
								   IServerConfigurationManager      configurationManager,
								   EncodingHelper                   encodingHelper) {
		_assembly = Assembly.GetExecutingAssembly();

		_libraryManager       = libraryManager;
		_logger               = logger;
		_fileSystem           = fileSystem;
		_loggerFactory        = loggerFactory;
		_appPaths             = appPaths;
		_libraryMonitor       = libraryMonitor;
		_mediaEncoder         = mediaEncoder;
		_configurationManager = configurationManager;
		_encodingHelper       = encodingHelper;

		_config = SmartPlaylistPlugin.Instance!.Configuration;
	}

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