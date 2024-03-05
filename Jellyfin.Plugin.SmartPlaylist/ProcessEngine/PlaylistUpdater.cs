﻿using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;

namespace Jellyfin.Plugin.SmartPlaylist.ProcessEngine;

public class PlaylistUpdater
{
	private readonly          User                             _user;
	private readonly          BaseItemKind[]                   _supportedItems;
	private readonly          IFileSystem                      _fileSystem;
	private readonly          ILibraryManager                  _libraryManager;
	private readonly          ILogger                          _logger;
	private readonly          IPlaylistManager                 _playlistManager;
	private readonly          IProviderManager                 _providerManager;
	private          readonly ProgressTracker                  _progress;
	private          readonly SmartPlaylistPluginConfiguration _config;

	public PlaylistUpdater(User                             user,
						   BaseItemKind[]                   supportedItems,
						   IFileSystem                      fileSystem,
						   ILibraryManager                  libraryManager,
						   IPlaylistManager                 playlistManager,
						   IProviderManager                 providerManager,
						   ILogger                          logger,
						   ProgressTracker                  progress,
						   SmartPlaylistPluginConfiguration config)
	{
		_user            = user;
		_fileSystem      = fileSystem;
		_supportedItems  = supportedItems;
		_libraryManager  = libraryManager;
		_logger          = logger;
		_playlistManager = playlistManager;
		_providerManager = providerManager;
		_progress        = progress;
		_config     = config;
	}

	private IReadOnlyList<BaseItem> GetAllUserMedia()
	{
		var query = new InternalItemsQuery(_user)
		{
			IncludeItemTypes = _supportedItems,
			Recursive = true,
		};

		return _libraryManager.GetItemsResult(query).Items;
	}

	public async Task ProcessPlaylists(IEnumerable<SmartPlaylistsRefreshJob> jobsIn,
									   CancellationToken                     cancellationToken)
	{

		var jobs          = jobsIn.ToArray();
		var userPlaylists = GetUserPlaylists().ToArray();
		var items         = GetAllUserMedia();

		//Set the percent to calculate all items though all playlists
		var totalThingsToProcess = items.Count * jobs.Length;

		//Add Each jobs separate steps
		totalThingsToProcess += jobs.Length * 3;

		var progress = new ProgressTracker(_progress, totalThingsToProcess);

		foreach (var job in jobs)
		{
			progress.Increment();
			job.BuildPlaylist(userPlaylists);
		}

		var options = new ParallelOptions
		{
			CancellationToken = cancellationToken,
		};

		if (_config.PlaylistSorterThreadCount is > 0 or -1)
		{
			options.MaxDegreeOfParallelism = _config.PlaylistSorterThreadCount;
		}
		else
		{
			options.MaxDegreeOfParallelism = 2;
		}

		ValueTask ParallelJobRun(BaseItem item, CancellationToken token)
		{
			var opp = new Operand(_libraryManager, item, BaseItem.UserDataManager, _user);

			foreach (var job in jobs)
			{
				progress.Increment();
				job.ProcessItem(opp);
			}

			return ValueTask.CompletedTask;
		}

		await Parallel.ForEachAsync(items, options, ParallelJobRun);

		foreach (var job in jobs)
		{
			progress.Increment();
			var newItems = job.GetItems();

			await job.CreateOrUpdatePlaylist(_playlistManager,
											 _providerManager,
											 _fileSystem,
											 newItems,
											 _config,
											 cancellationToken);
		}

		foreach (var job in jobs)
		{
			progress.Increment();

			if (job.HasErrors)
			{
				SmartPlaylistManager.SetErrorStatus(job.FileId, jobProcessErrors: job.ProcessErrors);
			}
			else
			{
				SmartPlaylistManager.SetStatus(job.FileId, SmartPlaylistLastRunDetails.SUCCESS, job.PlaylistId);
			}
		}
	}

	private IEnumerable<Playlist> GetUserPlaylists() => _playlistManager.GetPlaylists(_user.Id);
}
