using System.Text.Json;
using kgadi_ya_code_api.Models;

namespace kgadi_ya_code_api.Services;

public interface IOfflineService
{
    Task SaveUserDataAsync(Guid userId, object data);
    Task<T?> GetUserDataAsync<T>(Guid userId, string dataType);
    Task SyncWhenOnlineAsync();
    bool IsOnline { get; }
}

public class OfflineService : IOfflineService
{
    private readonly string _offlineDataPath;
    private readonly ILogger<OfflineService> _logger;
    private bool _isOnline = true;

    public OfflineService(ILogger<OfflineService> logger)
    {
        _logger = logger;
        _offlineDataPath = Path.Combine(Directory.GetCurrentDirectory(), "OfflineData");
        Directory.CreateDirectory(_offlineDataPath);
    }

    public bool IsOnline => _isOnline;

    public async Task SaveUserDataAsync(Guid userId, object data)
    {
        try
        {
            var userDir = Path.Combine(_offlineDataPath, userId.ToString());
            Directory.CreateDirectory(userDir);
            
            var fileName = $"{data.GetType().Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            var filePath = Path.Combine(userDir, fileName);
            
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            
            _logger.LogInformation("Saved offline data for user {UserId}: {FileName}", userId, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save offline data for user {UserId}", userId);
        }
    }

    public async Task<T?> GetUserDataAsync<T>(Guid userId, string dataType)
    {
        try
        {
            var userDir = Path.Combine(_offlineDataPath, userId.ToString());
            if (!Directory.Exists(userDir)) return default;

            var files = Directory.GetFiles(userDir, $"{dataType}_*.json")
                               .OrderByDescending(f => f)
                               .ToArray();

            if (!files.Any()) return default;

            var json = await File.ReadAllTextAsync(files.First());
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get offline data for user {UserId}", userId);
            return default;
        }
    }

    public Task SyncWhenOnlineAsync()
    {
        if (!_isOnline) return Task.CompletedTask;

        try
        {
            var userDirs = Directory.GetDirectories(_offlineDataPath);
            foreach (var userDir in userDirs)
            {
                var userId = Path.GetFileName(userDir);
                _logger.LogInformation("Syncing offline data for user {UserId}", userId);
                
                // Process sync files here when database is back online
                var syncFiles = Directory.GetFiles(userDir, "*.json");
                foreach (var file in syncFiles)
                {
                    // Mark as synced by moving to synced folder
                    var syncedDir = Path.Combine(userDir, "synced");
                    Directory.CreateDirectory(syncedDir);
                    var syncedFile = Path.Combine(syncedDir, Path.GetFileName(file));
                    File.Move(file, syncedFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync offline data");
        }
    }
}