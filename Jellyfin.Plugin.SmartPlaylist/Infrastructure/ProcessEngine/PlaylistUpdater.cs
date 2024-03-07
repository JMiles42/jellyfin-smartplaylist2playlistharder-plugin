using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using MediaBrowser.Controller.Playlists;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.ProcessEngine;

public class PlaylistUpdater
{
    private readonly User              _user;
    private readonly BaseItemKind[]    _supportedItems;
    private readonly IProgress<double> _progress;

    private readonly ILibraryManager                   _libraryManager;
    private readonly IPlaylistManager                  _playlistManager;
    private readonly ISmartPlaylistPluginConfiguration _config;
    private readonly IServiceProvider                  _serviceProvider;

    public PlaylistUpdater(User                              user,
                           BaseItemKind[]                    supportedItems,
                           IProgress<double>                 progress,
                           ILibraryManager                   libraryManager,
                           IPlaylistManager                  playlistManager,
                           ISmartPlaylistPluginConfiguration config,
                           IServiceProvider                  serviceProvider)
    {
        _user            = user;
        _supportedItems  = supportedItems;
        _libraryManager  = libraryManager;
        _playlistManager = playlistManager;
        _progress        = progress;
        _config          = config;
        _serviceProvider = serviceProvider;
    }

    private IReadOnlyList<BaseItem> GetAllUserMedia()
    {
        var query = new InternalItemsQuery(_user)
        {
            IncludeItemTypes = _supportedItems,
            Recursive        = true,
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
            job.SetupPlaylist(userPlaylists);
        }

        var options = new ParallelOptions
        {
            CancellationToken      = cancellationToken,
            MaxDegreeOfParallelism = GetThreadLimit(),
        };

        await Parallel.ForEachAsync(items, options, ProcessJobItems);

        foreach (var job in jobs)
        {
            progress.Increment();
            var newItems = job.GetItems();

            await job.CreateOrUpdatePlaylist(newItems, cancellationToken);
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

        return;

        ValueTask ProcessJobItems(BaseItem item, CancellationToken token)
        {
            var opp = _serviceProvider.GetRequiredService<OperandFactory>().Create(item, _user);

            foreach (var job in jobs)
            {
                progress.Increment();
                job.ProcessItem(opp);
            }

            return ValueTask.CompletedTask;
        }
    }

    private int GetThreadLimit()
    {
        if (_config.PlaylistSorterThreadCount is > 0 or -1)
        {
            return _config.PlaylistSorterThreadCount;
        }

        return 1;
    }

    private IEnumerable<Playlist> GetUserPlaylists() => _playlistManager.GetPlaylists(_user.Id);
}
