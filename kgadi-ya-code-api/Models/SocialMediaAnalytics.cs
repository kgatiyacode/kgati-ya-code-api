using System.ComponentModel.DataAnnotations;

namespace kgadi_ya_code_api.Models;

public class SocialMediaAnalytics
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid BusinessId { get; set; }
    
    public string Platform { get; set; } = ""; // Facebook, Twitter, TikTok, Instagram
    
    public int Followers { get; set; }
    
    public int Following { get; set; }
    
    public int Posts { get; set; }
    
    public int Engagement { get; set; }
    
    public decimal EngagementRate { get; set; }
    
    public int Reach { get; set; }
    
    public int Impressions { get; set; }
    
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    
    // Navigation properties
    public virtual Business Business { get; set; } = null!;
}

public class SocialMediaPost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid BusinessId { get; set; }
    
    public string Platform { get; set; } = "";
    
    public string PostId { get; set; } = "";
    
    public string Content { get; set; } = "";
    
    public List<string> MediaUrls { get; set; } = new();
    
    public int Likes { get; set; }
    
    public int Shares { get; set; }
    
    public int Comments { get; set; }
    
    public int Views { get; set; }
    
    public DateTime PostedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Business Business { get; set; } = null!;
}