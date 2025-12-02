using System.ComponentModel.DataAnnotations;

namespace kgadi_ya_code_api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required, EmailAddress]
    public string Email { get; set; } = "";
    
    [Required]
    public string PasswordHash { get; set; } = "";
    
    [Required]
    public string FirstName { get; set; } = "";
    
    [Required]
    public string LastName { get; set; } = "";
    
    public string? PhoneNumber { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsEmailVerified { get; set; } = false;
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiry { get; set; }
    
    // Navigation properties
    public virtual ICollection<Business> Businesses { get; set; } = new List<Business>();
}