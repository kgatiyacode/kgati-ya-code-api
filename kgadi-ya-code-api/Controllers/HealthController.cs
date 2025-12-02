using Microsoft.AspNetCore.Mvc;
using kgadi_ya_code_api.Services;

namespace kgadi_ya_code_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IResilienceService _resilienceService;
    private readonly IOfflineService _offlineService;

    public HealthController(IResilienceService resilienceService, IOfflineService offlineService)
    {
        _resilienceService = resilienceService;
        _offlineService = offlineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var isDatabaseHealthy = await _resilienceService.IsDatabaseHealthyAsync();
        
        var health = new
        {
            Status = isDatabaseHealthy ? "Healthy" : "Degraded",
            Database = isDatabaseHealthy ? "Online" : "Offline",
            OfflineMode = _offlineService.IsOnline ? "Disabled" : "Enabled",
            Timestamp = DateTime.UtcNow,
            Services = new
            {
                API = "Online",
                AI = "Online",
                Cache = "Online",
                SocialMedia = "Online"
            }
        };

        return isDatabaseHealthy ? Ok(health) : StatusCode(503, health);
    }

    [HttpPost("reconnect")]
    public async Task<IActionResult> TryReconnect()
    {
        var success = await _resilienceService.TryReconnectAsync();
        
        return Ok(new
        {
            Success = success,
            Message = success ? "Database reconnected successfully" : "Reconnection failed",
            Timestamp = DateTime.UtcNow
        });
    }
}