using RestSharp;
using System.Text.Json;
using kgadi_ya_code_api.Models;
using kgadi_ya_code_api.Data;
using Microsoft.EntityFrameworkCore;

namespace kgadi_ya_code_api.Services;

public interface ISocialMediaService
{
    Task<SocialMediaAnalytics> GetFacebookAnalyticsAsync(Guid businessId);
    Task<SocialMediaAnalytics> GetTwitterAnalyticsAsync(Guid businessId);
    Task<SocialMediaAnalytics> GetTikTokAnalyticsAsync(Guid businessId);
    Task<bool> PostToFacebookAsync(Guid businessId, string content, List<string> mediaUrls);
    Task<bool> PostToTwitterAsync(Guid businessId, string content, List<string> mediaUrls);
    Task<List<SocialMediaPost>> GetRecentPostsAsync(Guid businessId, string platform, int count = 10);
}

public class SocialMediaService : ISocialMediaService
{
    private readonly ApplicationDbContext _context;
    private readonly RestClient _restClient;

    public SocialMediaService(ApplicationDbContext context)
    {
        _context = context;
        _restClient = new RestClient();
    }

    public async Task<SocialMediaAnalytics> GetFacebookAnalyticsAsync(Guid businessId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business?.FacebookAccessToken == null)
            return CreateEmptyAnalytics(businessId, "Facebook");

        try
        {
            // Facebook Graph API call
            var request = new RestRequest($"https://graph.facebook.com/v18.0/me/insights", Method.Get);
            request.AddParameter("access_token", business.FacebookAccessToken);
            request.AddParameter("metric", "page_followers,page_impressions,page_engaged_users");

            var response = await _restClient.ExecuteAsync(request);
            
            if (response.IsSuccessful && response.Content != null)
            {
                var data = JsonSerializer.Deserialize<FacebookInsightsResponse>(response.Content);
                return MapFacebookAnalytics(businessId, data);
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Facebook API error: {ex.Message}");
        }

        return CreateEmptyAnalytics(businessId, "Facebook");
    }

    public async Task<SocialMediaAnalytics> GetTwitterAnalyticsAsync(Guid businessId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business?.TwitterAccessToken == null)
            return CreateEmptyAnalytics(businessId, "Twitter");

        try
        {
            // Twitter API v2 call
            var request = new RestRequest("https://api.twitter.com/2/users/me", Method.Get);
            request.AddHeader("Authorization", $"Bearer {business.TwitterAccessToken}");
            request.AddParameter("user.fields", "public_metrics");

            var response = await _restClient.ExecuteAsync(request);
            
            if (response.IsSuccessful && response.Content != null)
            {
                var data = JsonSerializer.Deserialize<TwitterUserResponse>(response.Content);
                return MapTwitterAnalytics(businessId, data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Twitter API error: {ex.Message}");
        }

        return CreateEmptyAnalytics(businessId, "Twitter");
    }

    public async Task<SocialMediaAnalytics> GetTikTokAnalyticsAsync(Guid businessId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business?.TikTokAccessToken == null)
            return CreateEmptyAnalytics(businessId, "TikTok");

        try
        {
            // TikTok Business API call
            var request = new RestRequest("https://business-api.tiktok.com/open_api/v1.3/user/info/", Method.Get);
            request.AddHeader("Access-Token", business.TikTokAccessToken);

            var response = await _restClient.ExecuteAsync(request);
            
            if (response.IsSuccessful && response.Content != null)
            {
                var data = JsonSerializer.Deserialize<TikTokUserResponse>(response.Content);
                return MapTikTokAnalytics(businessId, data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TikTok API error: {ex.Message}");
        }

        return CreateEmptyAnalytics(businessId, "TikTok");
    }

    public async Task<bool> PostToFacebookAsync(Guid businessId, string content, List<string> mediaUrls)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business?.FacebookAccessToken == null) return false;

        try
        {
            var request = new RestRequest("https://graph.facebook.com/v18.0/me/feed", Method.Post);
            request.AddParameter("access_token", business.FacebookAccessToken);
            request.AddParameter("message", content);

            if (mediaUrls.Any())
            {
                request.AddParameter("link", mediaUrls.First());
            }

            var response = await _restClient.ExecuteAsync(request);
            
            if (response.IsSuccessful)
            {
                // Save post record
                var post = new SocialMediaPost
                {
                    BusinessId = businessId,
                    Platform = "Facebook",
                    Content = content,
                    MediaUrls = mediaUrls,
                    PostedAt = DateTime.UtcNow
                };
                
                _context.SocialMediaPosts.Add(post);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Facebook post error: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> PostToTwitterAsync(Guid businessId, string content, List<string> mediaUrls)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business?.TwitterAccessToken == null) return false;

        try
        {
            var request = new RestRequest("https://api.twitter.com/2/tweets", Method.Post);
            request.AddHeader("Authorization", $"Bearer {business.TwitterAccessToken}");
            request.AddJsonBody(new { text = content });

            var response = await _restClient.ExecuteAsync(request);
            
            if (response.IsSuccessful)
            {
                var post = new SocialMediaPost
                {
                    BusinessId = businessId,
                    Platform = "Twitter",
                    Content = content,
                    MediaUrls = mediaUrls,
                    PostedAt = DateTime.UtcNow
                };
                
                _context.SocialMediaPosts.Add(post);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Twitter post error: {ex.Message}");
        }

        return false;
    }

    public async Task<List<SocialMediaPost>> GetRecentPostsAsync(Guid businessId, string platform, int count = 10)
    {
        return await _context.SocialMediaPosts
            .Where(p => p.BusinessId == businessId && p.Platform == platform)
            .OrderByDescending(p => p.PostedAt)
            .Take(count)
            .ToListAsync();
    }

    private SocialMediaAnalytics CreateEmptyAnalytics(Guid businessId, string platform)
    {
        return new SocialMediaAnalytics
        {
            BusinessId = businessId,
            Platform = platform,
            Date = DateTime.UtcNow
        };
    }

    private SocialMediaAnalytics MapFacebookAnalytics(Guid businessId, FacebookInsightsResponse? data)
    {
        return new SocialMediaAnalytics
        {
            BusinessId = businessId,
            Platform = "Facebook",
            Followers = data?.Data?.FirstOrDefault(d => d.Name == "page_followers")?.Values?.LastOrDefault()?.Value ?? 0,
            Impressions = data?.Data?.FirstOrDefault(d => d.Name == "page_impressions")?.Values?.LastOrDefault()?.Value ?? 0,
            Engagement = data?.Data?.FirstOrDefault(d => d.Name == "page_engaged_users")?.Values?.LastOrDefault()?.Value ?? 0,
            Date = DateTime.UtcNow
        };
    }

    private SocialMediaAnalytics MapTwitterAnalytics(Guid businessId, TwitterUserResponse? data)
    {
        return new SocialMediaAnalytics
        {
            BusinessId = businessId,
            Platform = "Twitter",
            Followers = data?.Data?.PublicMetrics?.FollowersCount ?? 0,
            Following = data?.Data?.PublicMetrics?.FollowingCount ?? 0,
            Posts = data?.Data?.PublicMetrics?.TweetCount ?? 0,
            Date = DateTime.UtcNow
        };
    }

    private SocialMediaAnalytics MapTikTokAnalytics(Guid businessId, TikTokUserResponse? data)
    {
        return new SocialMediaAnalytics
        {
            BusinessId = businessId,
            Platform = "TikTok",
            Followers = data?.Data?.FollowerCount ?? 0,
            Following = data?.Data?.FollowingCount ?? 0,
            Posts = data?.Data?.VideoCount ?? 0,
            Date = DateTime.UtcNow
        };
    }
}

// Response models for social media APIs
public class FacebookInsightsResponse
{
    public List<FacebookInsightData>? Data { get; set; }
}

public class FacebookInsightData
{
    public string Name { get; set; } = "";
    public List<FacebookInsightValue>? Values { get; set; }
}

public class FacebookInsightValue
{
    public int Value { get; set; }
    public string EndTime { get; set; } = "";
}

public class TwitterUserResponse
{
    public TwitterUserData? Data { get; set; }
}

public class TwitterUserData
{
    public TwitterPublicMetrics? PublicMetrics { get; set; }
}

public class TwitterPublicMetrics
{
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int TweetCount { get; set; }
}

public class TikTokUserResponse
{
    public TikTokUserData? Data { get; set; }
}

public class TikTokUserData
{
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int VideoCount { get; set; }
}