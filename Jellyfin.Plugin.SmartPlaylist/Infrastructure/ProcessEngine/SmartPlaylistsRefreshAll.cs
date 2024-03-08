using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.ProcessEngine;

public class SmartPlaylistsRefreshAll: ISmartPlaylistsRefreshAll
{
    private readonly ISmartPlaylistManager             _smartPlaylistManager;
    private readonly IServiceProvider                  _serviceProvider;
    private readonly ISmartPlaylistPluginConfiguration _config;

    public SmartPlaylistsRefreshAll(ISmartPlaylistManager             smartPlaylistManager,
                                    IServiceProvider                  serviceProvider,
                                    ISmartPlaylistPluginConfiguration config)
    {
        _smartPlaylistManager = smartPlaylistManager;
        _serviceProvider      = serviceProvider;
        _config               = config;
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

        int    i             = 0;

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

            var tracker = new NestedProgressTracker(progress,
                                                    1,
                                                    jobGroups.Length,
                                                    i,
                                                    jobGroups.Length);

            var sorter = _serviceProvider.GetRequiredService<PlaylistUpdaterFactory>()
                                         .BuildUpdater(group.Key.User, group.Key.Kinds, tracker);

            await sorter.ProcessPlaylists(group, cancellationToken).ConfigureAwait(false);

            i++;
            progress.Report((i + 1D) / jobGroups.Length);
        }
    }

    private GroupedItems[] GetGroupings(IEnumerable<SmartPlaylistsRefreshJob> jobs)
    {
        if (_config.PlaylistBatchedProcessing)
        {
            return jobs.GroupBy(a => a.GetGrouping())
                       .Select(a => new GroupedItems(a.Key, a))
                       .ToArray();
        }

        return jobs.Select(a => new GroupedItems(a.GetGrouping(), new[] { a, }))
                   .ToArray();
    }

    private class GroupedItems: IEnumerable<SmartPlaylistsRefreshJob>
    {

        public GroupedItems(JobGrouping key, IEnumerable<SmartPlaylistsRefreshJob> jobs)
        {
            Key  = key;
            Jobs = jobs;
        }

        public JobGrouping Key { get; }

        private IEnumerable<SmartPlaylistsRefreshJob> Jobs { get; }

        /// <inheritdoc />
        public IEnumerator<SmartPlaylistsRefreshJob> GetEnumerator() => Jobs.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Jobs).GetEnumerator();
    }
}
