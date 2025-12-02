using System.ComponentModel.DataAnnotations;

namespace kgadi_ya_code_api.Models;

public class Website
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = "";
    
    public string? Domain { get; set; }
    
    public string? Subdomain { get; set; }
    
    public string TemplateId { get; set; } = "";
    
    public string Theme { get; set; } = "default";
    
    public string? CustomCss { get; set; }
    
    public string? CustomJs { get; set; }
    
    public WebsiteConfig Config { get; set; } = new();
    
    public bool IsPublished { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? PublishedAt { get; set; }
    
    public Guid BusinessId { get; set; }
    
    // Navigation properties
    public virtual Business Business { get; set; } = null!;
    public virtual ICollection<WebsitePage> Pages { get; set; } = new List<WebsitePage>();
}

public class WebsiteConfig
{
    public string PrimaryColor { get; set; } = "#007bff";
    public string SecondaryColor { get; set; } = "#6c757d";
    public string FontFamily { get; set; } = "Arial, sans-serif";
    public bool ShowSocialLinks { get; set; } = true;
    public bool EnableEcommerce { get; set; } = true;
    public bool EnableBlog { get; set; } = false;
    public string ContactEmail { get; set; } = "";
    public string ContactPhone { get; set; } = "";
}

public class WebsitePage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Title { get; set; } = "";
    
    public string Slug { get; set; } = "";
    
    public string Content { get; set; } = "";
    
    public string? MetaDescription { get; set; }
    
    public List<string> MetaKeywords { get; set; } = new();
    
    public bool IsHomePage { get; set; } = false;
    
    public bool IsPublished { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid WebsiteId { get; set; }
    
    // Navigation properties
    public virtual Website Website { get; set; } = null!;
}