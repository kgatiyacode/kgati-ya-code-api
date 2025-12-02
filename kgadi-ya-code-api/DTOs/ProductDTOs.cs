using System.ComponentModel.DataAnnotations;

namespace kgadi_ya_code_api.DTOs;

public class CreateProductRequest
{
    [Required]
    public string Name { get; set; } = "";
    
    public string? Description { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public string? Category { get; set; }
    public string? SKU { get; set; }
    public int StockQuantity { get; set; }
    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    
    [Required]
    public Guid BusinessId { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public string? SKU { get; set; }
    public int? StockQuantity { get; set; }
    public List<string>? Images { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsActive { get; set; }
}

public class GenerateDescriptionRequest
{
    [Required]
    public string ProductName { get; set; } = "";
    
    public string? Category { get; set; }
    public List<string> Features { get; set; } = new();
    public string? TargetAudience { get; set; }
    public string? Tone { get; set; } = "professional"; // professional, casual, creative
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? AiGeneratedDescription { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public string? SKU { get; set; }
    public int StockQuantity { get; set; }
    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
}