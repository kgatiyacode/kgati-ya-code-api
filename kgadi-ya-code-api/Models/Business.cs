using System.ComponentModel.DataAnnotations;

namespace kgadi_ya_code_api.Models;

public class Business
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = "";
    
    public string? Description { get; set; }
    
    public string? Industry { get; set; }
    
    public string? Website { get; set; }
    
    public string? Logo { get; set; }
    
    public string? Address { get; set; }
    
    public string? City { get; set; }
    
    public string? Country { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? Email { get; set; }
    
    // Social Media Links
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? TikTokUrl { get; set; }
    public string? InstagramUrl { get; set; }
    
    // Social Media Access Tokens (encrypted)
    public string? FacebookAccessToken { get; set; }
    public string? TwitterAccessToken { get; set; }
    public string? TikTokAccessToken { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid UserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Website> Websites { get; set; } = new List<Website>();
    public virtual ICollection<SocialMediaAnalytics> SocialMediaAnalytics { get; set; } = new List<SocialMediaAnalytics>();
}