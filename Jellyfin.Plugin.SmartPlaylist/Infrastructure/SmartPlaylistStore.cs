using System.Text.Json.Serialization;
using Jellyfin.Plugin.SmartPlaylist.Interfaces;
using Jellyfin.Plugin.SmartPlaylist.Models.Dto;

namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure;

public class SmartPlaylistStore : ISmartPlaylistStore
{

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = {
                    new JsonStringEnumConverter(allowIntegerValues:true)
            },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    private readonly ISmartPlaylistFileSystem _fileSystem;

    public ILogger Logger { get; }

    public SmartPlaylistStore(ISmartPlaylistFileSystem fileSystem, ILogger logger)
    {
        Logger = logger;
        _fileSystem = fileSystem;
    }

    internal static async Task<SmartPlaylistDto> LoadPlaylistAsync(string filePath)
    {
        await using var reader = File.OpenRead(filePath);

        return (await JsonSerializer.DeserializeAsync<SmartPlaylistDto>(reader, _options).ConfigureAwait(false)).Validate();
    }

    public async Task<SmartPlaylistDto> GetSmartPlaylistAsync(Guid smartPlaylistId)
    {
        var fileName = _fileSystem.GetSmartPlaylistFilePath(smartPlaylistId.ToString());

        return (await LoadPlaylistAsync(fileName).ConfigureAwait(false)).Validate();
    }

    public async Task<SmartPlaylistDto[]> LoadPlaylistsAsync(Guid userId)
    {
        var deserializeTasks = _fileSystem.GetSmartPlaylistFilePaths(userId.ToString())
                                          .Select(LoadPlaylistAsync)
                                          .ToArray();

        await Task.WhenAll(deserializeTasks).ConfigureAwait(false);

        return deserializeTasks.Select(x => x.Result).ToArray();
    }

    public async Task<SmartPlaylistDto[]> GetAllSmartPlaylistAsync()
    {
        var deserializeTasks = _fileSystem.GetAllSmartPlaylistFilePaths().Select(LoadPlaylistAsync).ToArray();

        await Task.WhenAll(deserializeTasks).ConfigureAwait(false);

        return deserializeTasks.Select(x => x.Result.Validate()).ToArray();
    }

    public async Task SaveAsync(SmartPlaylistDto smartPList)
    {
        try
        {
            Logger.LogInformation("Saving playlistDto: {Name}", smartPList.Name);
            var filePath = _fileSystem.GetSmartPlaylistPath(smartPList.Id, smartPList.FileName);
            await using var writer = File.Create(filePath);
            await JsonSerializer.SerializeAsync(writer, smartPList, _options).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving playlistDto: {Name}", smartPList.Name);
        }
    }

    public void Delete(Guid userId, string smartPlaylistId)
    {
        var filePath = _fileSystem.GetSmartPlaylistPath(userId.ToString(), smartPlaylistId);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
