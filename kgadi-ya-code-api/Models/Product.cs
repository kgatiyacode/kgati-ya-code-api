using System.ComponentModel.DataAnnotations;

namespace kgadi_ya_code_api.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = "";
    
    public string? Description { get; set; }
    
    public string? AiGeneratedDescription { get; set; }
    
    public decimal Price { get; set; }
    
    public string? Category { get; set; }
    
    public string? SKU { get; set; }
    
    public int StockQuantity { get; set; }
    
    public List<string> Images { get; set; } = new();
    
    public List<string> Tags { get; set; } = new();
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public Guid BusinessId { get; set; }
    
    // Navigation properties
    public virtual Business Business { get; set; } = null!;
    public virtual ICollection<ProductAnalytics> ProductAnalytics { get; set; } = new List<ProductAnalytics>();
}

public class ProductAnalytics
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ProductId { get; set; }
    
    public string Platform { get; set; } = ""; // Facebook, Twitter, TikTok
    
    public int Views { get; set; }
    
    public int Likes { get; set; }
    
    public int Shares { get; set; }
    
    public int Comments { get; set; }
    
    public int Clicks { get; set; }
    
    public decimal ConversionRate { get; set; }
    
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}