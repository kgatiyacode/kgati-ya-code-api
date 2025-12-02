using RestSharp;
using System.Text.Json;
using kgadi_ya_code_api.DTOs;

namespace kgadi_ya_code_api.Services;

public interface IAIService
{
    Task<string> GenerateProductDescriptionAsync(GenerateDescriptionRequest request);
    Task<string> GenerateWebsiteContentAsync(string businessName, string industry, string pageType);
    Task<List<string>> SuggestProductTagsAsync(string productName, string category);
    Task<string> SimplifyTextAsync(string text, string userLevel = "beginner");
    Task<string> ChatWithLLMAsync(string userMessage, string context = "");
    Task<List<string>> GetTrainingDataSuggestionsAsync(string industry, string businessType);
}

public class AIService : IAIService
{
    private readonly RestClient _restClient;
    private readonly string _apiKey;
    private readonly ICacheService _cache;
    private readonly ILogger<AIService> _logger;

    public AIService(IConfiguration configuration, ICacheService cache, ILogger<AIService> logger)
    {
        _restClient = new RestClient("https://generativelanguage.googleapis.com");
        _apiKey = configuration["GoogleAI:ApiKey"] ?? "";
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GenerateProductDescriptionAsync(GenerateDescriptionRequest request)
    {
        var prompt = $"Generate a compelling product description for {request.ProductName} in {request.Category ?? "General"} category. Features: {string.Join(", ", request.Features)}. Target: {request.TargetAudience ?? "General consumers"}. Tone: {request.Tone}. Keep it 100-200 words, SEO-friendly.";

        try
        {
            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            var restRequest = new RestRequest($"/v1beta/models/gemini-pro:generateContent?key={_apiKey}", Method.Post);
            restRequest.AddJsonBody(requestBody);

            var response = await _restClient.ExecuteAsync(restRequest);
            
            if (response.IsSuccessful && response.Content != null)
            {
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(response.Content);
                return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? GetFallbackDescription(request);
            }
        }
        catch { }

        return await GetCachedFallback($"desc_{request.ProductName}", () => GetFallbackDescription(request));
    }

    private string GetFallbackDescription(GenerateDescriptionRequest request)
    {
        return $"Discover the amazing {request.ProductName} - a high-quality {request.Category?.ToLower()} designed to meet your needs. " +
               $"With exceptional features and reliable performance, this product offers great value for {request.TargetAudience?.ToLower() ?? "everyone"}. " +
               $"Experience the difference with {request.ProductName}.";
    }

    private async Task<string> GetCachedFallback(string key, Func<string> fallback)
    {
        try
        {
            return await _cache.GetOrSetAsync(key, () => Task.FromResult(fallback()), TimeSpan.FromHours(24));
        }
        catch
        {
            return fallback();
        }
    }

    public async Task<string> GenerateWebsiteContentAsync(string businessName, string industry, string pageType)
    {
        var prompt = $"Generate {pageType} page content for {businessName} in {industry} industry. Professional tone, 200-400 words, include call-to-action, SEO-friendly, focus on customer benefits.";

        try
        {
            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            var restRequest = new RestRequest($"/v1beta/models/gemini-pro:generateContent?key={_apiKey}", Method.Post);
            restRequest.AddJsonBody(requestBody);

            var response = await _restClient.ExecuteAsync(restRequest);
            
            if (response.IsSuccessful && response.Content != null)
            {
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(response.Content);
                return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? GetFallbackContent(businessName, industry, pageType);
            }
        }
        catch { }

        return GetFallbackContent(businessName, industry, pageType);
    }

    private string GetFallbackContent(string businessName, string industry, string pageType)
    {
        return pageType.ToLower() switch
        {
            "home" => $"Welcome to {businessName}, your trusted partner in {industry}. We provide exceptional services and products to help you achieve your goals.",
            "about" => $"{businessName} is a leading company in the {industry} industry, committed to delivering excellence and innovation to our customers.",
            "contact" => $"Get in touch with {businessName} today. We're here to help you with all your {industry} needs.",
            _ => $"Learn more about {businessName} and our {industry} services."
        };
    }

    public async Task<List<string>> SuggestProductTagsAsync(string productName, string category)
    {
        var prompt = $"Generate 8-10 SEO-friendly tags for {productName} in {category} category. Return only tags separated by commas. Single words or short phrases, no special characters.";

        try
        {
            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            var restRequest = new RestRequest($"/v1beta/models/gemini-pro:generateContent?key={_apiKey}", Method.Post);
            restRequest.AddJsonBody(requestBody);

            var response = await _restClient.ExecuteAsync(restRequest);
            
            if (response.IsSuccessful && response.Content != null)
            {
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(response.Content);
                var tagsString = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "";
                return tagsString.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }
        }
        catch { }

        // Fallback tags
        var fallbackTags = new List<string> { category?.ToLower() ?? "product", "quality", "affordable", "popular" };
        if (!string.IsNullOrEmpty(productName))
        {
            fallbackTags.AddRange(productName.Split(' ').Take(3).Select(w => w.ToLower()));
        }
        return fallbackTags.Distinct().ToList();
    }

    public async Task<string> SimplifyTextAsync(string text, string userLevel = "beginner")
    {
        var cacheKey = $"simplify_{text.GetHashCode()}_{userLevel}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var prompt = $"Simplify this text for a {userLevel} level user. Make it easy to understand: {text}";
            return await CallGeminiAPI(prompt) ?? $"Here's a simpler version: {text.Substring(0, Math.Min(100, text.Length))}...";
        }, TimeSpan.FromHours(6));
    }

    public async Task<string> ChatWithLLMAsync(string userMessage, string context = "")
    {
        var prompt = $"Context: {context}\n\nUser: {userMessage}\n\nRespond helpfully as a business assistant:";
        return await CallGeminiAPI(prompt) ?? "I'm here to help! Could you please rephrase your question?";
    }

    public async Task<List<string>> GetTrainingDataSuggestionsAsync(string industry, string businessType)
    {
        var cacheKey = $"training_{industry}_{businessType}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var prompt = $"Suggest 10 data sources for training AI models for {businessType} businesses in {industry}. Include free and paid options.";
            var response = await CallGeminiAPI(prompt);
            
            if (response != null)
            {
                return response.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Take(10)
                    .ToList();
            }
            
            return GetFallbackTrainingData(industry, businessType);
        }, TimeSpan.FromDays(1));
    }

    private async Task<string?> CallGeminiAPI(string prompt)
    {
        try
        {
            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            var restRequest = new RestRequest($"/v1beta/models/gemini-pro:generateContent?key={_apiKey}", Method.Post);
            restRequest.AddJsonBody(requestBody);

            var response = await _restClient.ExecuteAsync(restRequest);
            
            if (response.IsSuccessful && response.Content != null)
            {
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(response.Content);
                return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed for prompt: {Prompt}", prompt.Substring(0, Math.Min(50, prompt.Length)));
        }
        return null;
    }

    private List<string> GetFallbackTrainingData(string industry, string businessType)
    {
        return new List<string>
        {
            "Google Analytics - Website traffic data",
            "Social media platform APIs - Engagement metrics",
            "Customer surveys - Direct feedback",
            "Industry reports - Market trends",
            "Competitor analysis - Benchmarking data",
            "Sales records - Transaction patterns",
            "Customer support tickets - Common issues",
            "Product reviews - User sentiment",
            "Email marketing metrics - Campaign performance",
            "Search engine data - Keyword trends"
        };
    }
}

// Gemini API response models
public class GeminiResponse
{
    public List<GeminiCandidate>? Candidates { get; set; }
}

public class GeminiCandidate
{
    public GeminiContent? Content { get; set; }
}

public class GeminiContent
{
    public List<GeminiPart>? Parts { get; set; }
}

public class GeminiPart
{
    public string? Text { get; set; }
}