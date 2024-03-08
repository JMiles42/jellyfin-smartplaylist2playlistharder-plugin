using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.ProcessEngine;

public class SmartPlaylistsRefreshAll: ISmartPlaylistsRefreshAll
{
    private readonly ISmartPlaylistManager             _smartPlaylistManager;
    private readonly IUserManager                      _userManager;
    private readonly IServiceProvider                  _serviceProvider;
    private readonly ISmartPlaylistPluginConfiguration _config;

    public SmartPlaylistsRefreshAll(IUserManager                      userManager,
                                    ISmartPlaylistManager             smartPlaylistManager,
                                    IServiceProvider                  serviceProvider,
                                    ISmartPlaylistPluginConfiguration config)
    {
        _smartPlaylistManager = smartPlaylistManager;
        _userManager          = userManager;
        _serviceProvider      = serviceProvider;
        this._config           = config;
    }

    public async Task ExecuteAsync(IProgress<double> progress,
                                   CancellationToken cancellationToken)
    {
        var dtos = _smartPlaylistManager.GetAllPlaylists()
                                        .ToArray();

        var jobs = dtos
                   .Select(d => _serviceProvider.GetRequiredService<SmartPlaylistsRefreshJobFactory>().BuildJob(d))
                   .ToArray();

        foreach (var job in jobs)
        {
            job.SetUser();
        }

        var jobGroups = GetGroupings(jobs);

        var tracker = new ProgressTracker(progress, jobGroups.Length);

        foreach (var group in jobGroups)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (group.Key.User is null || group.Key.Kinds is null)
            {
                foreach (var job in group)
                {
                    SmartPlaylistManager.SetErrorStatus(job.FileId, jobProcessErrors: job.ProcessErrors);
                }

                continue;
            }

            var sorter = _serviceProvider.GetRequiredService<PlaylistUpdaterFactory>()
                                         .BuildUpdater(group.Key.User, group.Key.Kinds, tracker);

            await sorter.ProcessPlaylists(group, cancellationToken).ConfigureAwait(false);

            tracker.Increment();
        }
    }

    private GroupedItems[] GetGroupings(IEnumerable<SmartPlaylistsRefreshJob> jobs)
    {
        if (_config.PlaylistBatchedProcessing)
        {
            return jobs.GroupBy(a => a.GetGrouping())
                       .Select(a => new GroupedItems()
                       {
                           Key  = a.Key,
                           Jobs = a,
                       }).ToArray();
        }

        return jobs.Select(a => new GroupedItems()
                   {
                       Jobs = new[] { a, },
                       Key  = a.GetGrouping(),
                   })
                   .ToArray();
    }

    class GroupedItems : IEnumerable<SmartPlaylistsRefreshJob> {

        public JobGrouping                           Key  { get; init; }
        public IEnumerable<SmartPlaylistsRefreshJob> Jobs { get; init; }

        /// <inheritdoc />
        public IEnumerator<SmartPlaylistsRefreshJob> GetEnumerator() => Jobs.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Jobs).GetEnumerator();
    }
}
