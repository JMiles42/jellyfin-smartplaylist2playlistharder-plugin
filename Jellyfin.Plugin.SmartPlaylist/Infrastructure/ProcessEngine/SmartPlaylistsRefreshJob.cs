using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Playlists;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.ProcessEngine;

public class SmartPlaylistsRefreshJob
{
    private readonly ILogger                           _logger;
    private readonly ISmartPlaylistManager             _smartPlaylistManager;
    private readonly IPlaylistManager                  _playlistManager;
    private readonly IProviderManager                  _providerManager;
    private readonly IDirectoryService                 _directoryService;
    private readonly ISmartPlaylistPluginConfiguration _config;
    private readonly IUserManager                      _userManager;

    public PlaylistProcessRunData PlaylistProcessRunData { get; set; } = default!;

    public SmartPlaylistDto? SmartPlaylistDto { get; set; }

    public Guid? PlaylistId { get; set; }

    public Playlist? JellyfinPlaylist { get; set; }

    public Sorter? Sorter { get; set; }

    public List<SmartPlaylistsRefreshError> ProcessErrors { get; } = new();

    public string FileId => PlaylistProcessRunData.FileId;

    public bool HasErrors { get; private set; }

    public User? User { get; private set; }

    public JobGrouping? JobGrouping { get; private set; }

    public BaseItemKind[]? SupportedItems => SmartPlaylistDto?.SupportedItems;

    public SmartPlaylistsRefreshJob(PlaylistProcessRunData            playlistProcessRunData,
                                    ILogger<SmartPlaylistsRefreshJob> logger,
                                    IPlaylistManager                  playlistManager,
                                    ISmartPlaylistManager             smartPlaylistManager,
                                    IProviderManager                  providerManager,
                                    IDirectoryService                 directoryService,
                                    ISmartPlaylistPluginConfiguration config,
                                    IUserManager                      userManager)
    {
        _logger               = logger;
        _smartPlaylistManager = smartPlaylistManager;
        _playlistManager      = playlistManager;
        _providerManager      = providerManager;
        _directoryService     = directoryService;
        _config               = config;
        _userManager          = userManager;

        PlaylistProcessRunData = playlistProcessRunData;
        SmartPlaylistDto       = playlistProcessRunData.SmartPlaylist;
        PlaylistId             = playlistProcessRunData.SmartPlaylist?.Id;

        if (PlaylistProcessRunData.ErrorDetails is not null)
        {
            SetError("Parsing Error", PlaylistProcessRunData.ErrorDetails);

            return;
        }

        if (SmartPlaylistDto?.IsReadonly is true)
        {
            SetError("File is Readonly");
        }
    }

    private void SetError(string errorDetails, Exception? exception = null)
    {
        ProcessErrors.Add(new(errorDetails, exception));
        HasErrors = true;
    }

    public void SetUser()
    {
        if (string.IsNullOrEmpty(PlaylistProcessRunData.SmartPlaylist?.User))
        {
            SetError("Error User is null or empty, cannot process playlist.");

            return;
        }

        var username = PlaylistProcessRunData.SmartPlaylist.User;

        try
        {
            var user = _userManager.GetUserByName(username);

            if (user is null)
            {
                SetError($"Error User {username} does not exist, cannot process playlist.");
            }

            User = user;
        }
        catch (Exception e)
        {
            SetError($"Error when looking for User '{username}' cannot process playlist.", e);
        }
    }

    public JobGrouping GetGrouping() => JobGrouping ??= new(User, SupportedItems);

    public void SetupPlaylist(Playlist[] userPlaylists)
    {
        if (HasErrors)
        {
            return;
        }

        if (SmartPlaylistDto is null)
        {
            return;
        }

        if (CompilePlaylist(out var pl))
        {
            return;
        }

        if (pl is null || SetSorter(pl))
        {
            return;
        }

        SetJellyfinPlaylist(userPlaylists);
    }

    private bool CompilePlaylist(out Models.SmartPlaylist? pl)
    {
        try
        {
            pl                                = new(SmartPlaylistDto);
            pl.CompiledPlaylistExpressionSets = pl.CompilePlaylistExpressionSets();
        }
        catch (Exception ex)
        {
            SetError("Playlist failed to compile", ex);

            _logger.LogError(ex, "Error parsing rules for {FileId}", FileId);
            pl = null;

            return true;
        }

        return false;
    }

    private bool SetSorter(Models.SmartPlaylist pl)
    {
        try
        {
            Sorter = pl.GetSorter();
        }
        catch (Exception ex)
        {
            SetError("Playlist failed to get sorter", ex);

            _logger.LogError(ex, "Error parsing rules for {FileId}", FileId);

            return true;
        }

        return false;
    }

    private void SetJellyfinPlaylist(IEnumerable<Playlist> userPlaylists)
    {
        try
        {
            JellyfinPlaylist = userPlaylists.FirstOrDefault(a => a.Id == PlaylistId);
        }
        catch (Exception ex)
        {
            SetError("Error finding the playlist");

            _logger.LogError(ex, "Error finding Jellyfin Playlist {FileId}", FileId);
        }
    }

    public void ProcessItem(Operand item)
    {
        if (HasErrors)
        {
            return;
        }

        try
        {
            Sorter?.SortItem(item);
        }
        catch (Exception e)
        {
            SetError($"Error sorting item {item.Name}", e);
        }
    }

    public BaseItem[] GetItems()
    {
        if (HasErrors)
        {
            return Array.Empty<BaseItem>();
        }

        if (Sorter is null)
        {
            return Array.Empty<BaseItem>();
        }

        if (SmartPlaylistDto?.MaxItems > 0)
        {
            return Sorter.GetResults()
                         .Take(SmartPlaylistDto.MaxItems)
                         .ToArray();
        }

        return Sorter.GetResults()
                     .ToArray();
    }

    public async Task CreateOrUpdatePlaylist(BaseItem[]        items,
                                             CancellationToken token)
    {
        if (HasErrors)
        {
            return;
        }

        if (SmartPlaylistDto is null)
        {
            return;
        }

        if (User is null)
        {
            return;
        }


        if (JellyfinPlaylist is null)
        {
            await CreatePlaylist(items);
        }
        else
        {
            await UpdatePlaylist(items, token);
        }
    }

    private async Task UpdatePlaylist(BaseItem[]        items,
                                      CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Clearing and adding {Number} files to existing playlist: {FileId}",
                                   items.Length,
                                   FileId);

            JellyfinPlaylist!.LinkedChildren = items.Select(LinkedChild.Create)
                                                    .ToArray();

            await JellyfinPlaylist.UpdateToRepositoryAsync(ItemUpdateType.MetadataEdit, token)
                                  .ConfigureAwait(false);

            _providerManager.QueueRefresh(JellyfinPlaylist.Id,
                                          new(_directoryService)
                                          {
                                              ForceSave        = true,
                                              ImageRefreshMode = MetadataRefreshMode.FullRefresh,
                                          },
                                          RefreshPriority.High);

            if (_config.AlwaysSaveFile)
            {
                SavePlaylist();
            }
        }
        catch (Exception e)
        {
            SetError($"Error updating Playlist {FileId}", e);
        }
    }

    private async Task CreatePlaylist(BaseItem[] items)
    {
        try
        {
            _logger.LogInformation("Creating and Saving Playlist {FileId}", FileId);

            var req = new PlaylistCreationRequest
            {
                Name       = SmartPlaylistDto!.Name,
                UserId     = User!.Id,
                ItemIdList = items.Select(baseItem => baseItem.Id).ToArray(),
            };

            var foo = await _playlistManager.CreatePlaylist(req);
            PlaylistId          = Guid.Parse(foo.Id);
            SmartPlaylistDto.Id = PlaylistId;
            SavePlaylist();
        }
        catch (Exception e)
        {
            SetError($"Error creating Playlist {FileId}", e);
        }
    }

    private void SavePlaylist()
    {
        if (string.IsNullOrEmpty(SmartPlaylistDto?.FileName))
        {
            SetError("Error filename is null, cannot save playlist to disk");
        }
        else
        {
            _logger.LogInformation("Saving playlist {FileId}", FileId);
            _smartPlaylistManager.SavePlaylist(SmartPlaylistDto.FileName, SmartPlaylistDto);
        }
    }
}
