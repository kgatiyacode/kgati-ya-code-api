using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using kgadi_ya_code_api.Data;
using kgadi_ya_code_api.Services;
using System.Security.Claims;

namespace kgadi_ya_code_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SocialMediaController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISocialMediaService _socialMediaService;

    public SocialMediaController(ApplicationDbContext context, ISocialMediaService socialMediaService)
    {
        _context = context;
        _socialMediaService = socialMediaService;
    }

    [HttpGet("analytics/{businessId}")]
    public async Task<IActionResult> GetAnalytics(Guid businessId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Verify business ownership
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        var analytics = new
        {
            Facebook = await _socialMediaService.GetFacebookAnalyticsAsync(businessId),
            Twitter = await _socialMediaService.GetTwitterAnalyticsAsync(businessId),
            TikTok = await _socialMediaService.GetTikTokAnalyticsAsync(businessId)
        };

        return Ok(analytics);
    }

    [HttpGet("analytics/{businessId}/{platform}")]
    public async Task<IActionResult> GetPlatformAnalytics(Guid businessId, string platform)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Verify business ownership
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        var analytics = platform.ToLower() switch
        {
            "facebook" => await _socialMediaService.GetFacebookAnalyticsAsync(businessId),
            "twitter" => await _socialMediaService.GetTwitterAnalyticsAsync(businessId),
            "tiktok" => await _socialMediaService.GetTikTokAnalyticsAsync(businessId),
            _ => null
        };

        if (analytics == null)
            return BadRequest("Invalid platform");

        return Ok(analytics);
    }

    [HttpPost("post/{businessId}")]
    public async Task<IActionResult> CreatePost(Guid businessId, [FromBody] CreatePostRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Verify business ownership
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        var results = new Dictionary<string, bool>();

        if (request.Platforms.Contains("Facebook", StringComparer.OrdinalIgnoreCase))
        {
            results["Facebook"] = await _socialMediaService.PostToFacebookAsync(businessId, request.Content, request.MediaUrls);
        }

        if (request.Platforms.Contains("Twitter", StringComparer.OrdinalIgnoreCase))
        {
            results["Twitter"] = await _socialMediaService.PostToTwitterAsync(businessId, request.Content, request.MediaUrls);
        }

        return Ok(new { results, message = "Post creation completed" });
    }

    [HttpGet("posts/{businessId}")]
    public async Task<IActionResult> GetRecentPosts(Guid businessId, [FromQuery] string? platform = null, [FromQuery] int count = 10)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Verify business ownership
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        if (!string.IsNullOrEmpty(platform))
        {
            var posts = await _socialMediaService.GetRecentPostsAsync(businessId, platform, count);
            return Ok(posts);
        }

        var allPosts = new
        {
            Facebook = await _socialMediaService.GetRecentPostsAsync(businessId, "Facebook", count),
            Twitter = await _socialMediaService.GetRecentPostsAsync(businessId, "Twitter", count),
            TikTok = await _socialMediaService.GetRecentPostsAsync(businessId, "TikTok", count)
        };

        return Ok(allPosts);
    }

    [HttpGet("historical-analytics/{businessId}")]
    public async Task<IActionResult> GetHistoricalAnalytics(Guid businessId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Verify business ownership
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var analytics = await _context.SocialMediaAnalytics
            .Where(a => a.BusinessId == businessId && a.Date >= startDate && a.Date <= endDate)
            .OrderBy(a => a.Date)
            .GroupBy(a => a.Platform)
            .Select(g => new
            {
                Platform = g.Key,
                Data = g.Select(a => new
                {
                    a.Date,
                    a.Followers,
                    a.Following,
                    a.Posts,
                    a.Engagement,
                    a.EngagementRate,
                    a.Reach,
                    a.Impressions
                }).ToList()
            })
            .ToListAsync();

        return Ok(analytics);
    }
}

public class CreatePostRequest
{
    public string Content { get; set; } = "";
    public List<string> MediaUrls { get; set; } = new();
    public List<string> Platforms { get; set; } = new();
}