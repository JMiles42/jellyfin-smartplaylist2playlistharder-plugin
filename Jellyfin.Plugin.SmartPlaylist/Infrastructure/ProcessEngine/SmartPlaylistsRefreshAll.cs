using Jellyfin.Plugin.SmartPlaylist.Infrastructure.Factories;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.ProcessEngine;

public class SmartPlaylistsRefreshAll: ISmartPlaylistsRefreshAll
{
    private readonly ISmartPlaylistManager             _smartPlaylistManager;
    private readonly IUserManager                      _userManager;
    private readonly IServiceProvider                  _serviceProvider;

    public SmartPlaylistsRefreshAll(IUserManager                      userManager,
                                    ISmartPlaylistManager             smartPlaylistManager,
                                    IServiceProvider                  serviceProvider)
    {
        _smartPlaylistManager = smartPlaylistManager;
        _userManager          = userManager;
        _serviceProvider      = serviceProvider;
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

        var jobGroups = jobs.GroupBy(a => a.GetGrouping()).ToArray();

        var tracker = new ProgressTracker(progress, jobGroups.Length);

        foreach (var group in jobGroups)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
}
