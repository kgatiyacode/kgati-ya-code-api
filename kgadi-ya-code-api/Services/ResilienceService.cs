using Microsoft.EntityFrameworkCore;
using kgadi_ya_code_api.Data;
using System.Text.Json;

namespace kgadi_ya_code_api.Services;

public interface IResilienceService
{
    Task<bool> IsDatabaseHealthyAsync();
    Task<T?> ExecuteWithFallbackAsync<T>(Func<Task<T>> databaseOperation, Func<Task<T>> fallbackOperation);
    Task HandleDatabaseFailureAsync(Exception ex);
    Task<bool> TryReconnectAsync();
}

public class ResilienceService : IResilienceService
{
    private readonly ApplicationDbContext _context;
    private readonly IOfflineService _offlineService;
    private readonly ICacheService _cache;
    private readonly ILogger<ResilienceService> _logger;
    private bool _isDatabaseHealthy = true;
    private DateTime _lastHealthCheck = DateTime.MinValue;

    public ResilienceService(
        ApplicationDbContext context, 
        IOfflineService offlineService,
        ICacheService cache,
        ILogger<ResilienceService> logger)
    {
        _context = context;
        _offlineService = offlineService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsDatabaseHealthyAsync()
    {
        if (DateTime.UtcNow - _lastHealthCheck < TimeSpan.FromMinutes(1))
            return _isDatabaseHealthy;

        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            _isDatabaseHealthy = true;
            _lastHealthCheck = DateTime.UtcNow;
            
            if (!_isDatabaseHealthy)
            {
                _logger.LogInformation("Database connection restored");
                await _offlineService.SyncWhenOnlineAsync();
            }
        }
        catch (Exception ex)
        {
            _isDatabaseHealthy = false;
            _lastHealthCheck = DateTime.UtcNow;
            _logger.LogError(ex, "Database health check failed");
        }

        return _isDatabaseHealthy;
    }

    public async Task<T?> ExecuteWithFallbackAsync<T>(Func<Task<T>> databaseOperation, Func<Task<T>> fallbackOperation)
    {
        try
        {
            if (await IsDatabaseHealthyAsync())
            {
                return await databaseOperation();
            }
        }
        catch (Exception ex)
        {
            await HandleDatabaseFailureAsync(ex);
        }

        _logger.LogWarning("Using fallback operation due to database unavailability");
        return await fallbackOperation();
    }

    public async Task HandleDatabaseFailureAsync(Exception ex)
    {
        _isDatabaseHealthy = false;
        _logger.LogError(ex, "Database operation failed, switching to offline mode");
        
        // Notify monitoring systems
        await NotifySystemAdminsAsync(ex);
        
        // Attempt automatic recovery
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(5));
            await TryReconnectAsync();
        });
    }

    public async Task<bool> TryReconnectAsync()
    {
        try
        {
            _logger.LogInformation("Attempting database reconnection...");
            
            // Try to reconnect
            await _context.Database.OpenConnectionAsync();
            await _context.Database.CloseConnectionAsync();
            
            _isDatabaseHealthy = true;
            _logger.LogInformation("Database reconnection successful");
            
            // Sync offline data
            await _offlineService.SyncWhenOnlineAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database reconnection failed");
            return false;
        }
    }

    private async Task NotifySystemAdminsAsync(Exception ex)
    {
        try
        {
            // In production, send email/SMS/Slack notification
            var notification = new
            {
                Timestamp = DateTime.UtcNow,
                Service = "Kgati Ya Code API",
                Error = ex.Message,
                StackTrace = ex.StackTrace,
                Severity = "Critical"
            };

            _logger.LogCritical("SYSTEM ALERT: Database failure - {Error}", ex.Message);
            
            // Store notification for later processing
            await _cache.SetAsync($"alert_{DateTime.UtcNow:yyyyMMddHHmmss}", notification, TimeSpan.FromDays(7));
        }
        catch (Exception notificationEx)
        {
            _logger.LogError(notificationEx, "Failed to send system notification");
        }
    }
}