using System.ComponentModel.DataAnnotations;

namespace kgadi_ya_code_api.DTOs;

public class CreateBusinessRequest
{
    [Required]
    public string Name { get; set; } = "";
    
    public string? Description { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public class UpdateBusinessRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Logo { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? TikTokUrl { get; set; }
    public string? InstagramUrl { get; set; }
}

public class BusinessDto
{
    public Guid Id { get; set; }
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
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? TikTokUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProductCount { get; set; }
    public int WebsiteCount { get; set; }
}