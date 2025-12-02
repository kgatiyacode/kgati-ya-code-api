using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using kgadi_ya_code_api.Data;
using kgadi_ya_code_api.Models;
using kgadi_ya_code_api.Services;
using System.Security.Claims;

namespace kgadi_ya_code_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WebsiteController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAIService _aiService;

    public WebsiteController(ApplicationDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetWebsites(Guid businessId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        var websites = await _context.Websites
            .Where(w => w.BusinessId == businessId)
            .Include(w => w.Pages)
            .ToListAsync();

        return Ok(websites);
    }

    [HttpPost("business/{businessId}")]
    public async Task<IActionResult> CreateWebsite(Guid businessId, [FromBody] CreateWebsiteRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        var website = new Website
        {
            Name = request.Name,
            Domain = request.Domain,
            Subdomain = request.Subdomain,
            TemplateId = request.TemplateId,
            Theme = request.Theme ?? "default",
            Config = request.Config ?? new WebsiteConfig(),
            BusinessId = businessId
        };

        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // Create default pages
        var defaultPages = new[]
        {
            new WebsitePage { Title = "Home", Slug = "", IsHomePage = true, WebsiteId = website.Id, SortOrder = 1 },
            new WebsitePage { Title = "About", Slug = "about", WebsiteId = website.Id, SortOrder = 2 },
            new WebsitePage { Title = "Products", Slug = "products", WebsiteId = website.Id, SortOrder = 3 },
            new WebsitePage { Title = "Contact", Slug = "contact", WebsiteId = website.Id, SortOrder = 4 }
        };

        // Generate AI content for pages
        foreach (var page in defaultPages)
        {
            try
            {
                page.Content = await _aiService.GenerateWebsiteContentAsync(business.Name, business.Industry ?? "Business", page.Title);
            }
            catch
            {
                page.Content = $"Welcome to our {page.Title} page. Content coming soon!";
            }
        }

        _context.WebsitePages.AddRange(defaultPages);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWebsite), new { id = website.Id }, website);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWebsite(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var website = await _context.Websites
            .Include(w => w.Pages)
            .Include(w => w.Business)
            .FirstOrDefaultAsync(w => w.Id == id && w.Business.UserId == userId);

        if (website == null)
            return NotFound();

        return Ok(website);
    }

    [HttpPut("{id}/publish")]
    public async Task<IActionResult> PublishWebsite(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var website = await _context.Websites
            .Include(w => w.Business)
            .FirstOrDefaultAsync(w => w.Id == id && w.Business.UserId == userId);

        if (website == null)
            return NotFound();

        website.IsPublished = true;
        website.PublishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Website published successfully", publishedUrl = $"https://{website.Subdomain}.kgati.com" });
    }

    [HttpGet("templates")]
    public IActionResult GetTemplates()
    {
        var templates = new[]
        {
            new { Id = "modern-business", Name = "Modern Business", Category = "Business", Preview = "/templates/modern-business.jpg" },
            new { Id = "ecommerce-store", Name = "E-commerce Store", Category = "E-commerce", Preview = "/templates/ecommerce-store.jpg" },
            new { Id = "restaurant", Name = "Restaurant", Category = "Food & Beverage", Preview = "/templates/restaurant.jpg" },
            new { Id = "portfolio", Name = "Portfolio", Category = "Creative", Preview = "/templates/portfolio.jpg" },
            new { Id = "landing-page", Name = "Landing Page", Category = "Marketing", Preview = "/templates/landing-page.jpg" }
        };

        return Ok(templates);
    }
}

public class CreateWebsiteRequest
{
    public string Name { get; set; } = "";
    public string? Domain { get; set; }
    public string? Subdomain { get; set; }
    public string TemplateId { get; set; } = "modern-business";
    public string? Theme { get; set; }
    public WebsiteConfig? Config { get; set; }
}