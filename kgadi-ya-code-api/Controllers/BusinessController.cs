using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using kgadi_ya_code_api.Data;
using kgadi_ya_code_api.Models;
using kgadi_ya_code_api.DTOs;
using System.Security.Claims;

namespace kgadi_ya_code_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BusinessController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetBusinesses()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var businesses = await _context.Businesses
            .Where(b => b.UserId == userId)
            .Select(b => new BusinessDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Industry = b.Industry,
                Website = b.Website,
                Logo = b.Logo,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                PhoneNumber = b.PhoneNumber,
                Email = b.Email,
                FacebookUrl = b.FacebookUrl,
                TwitterUrl = b.TwitterUrl,
                TikTokUrl = b.TikTokUrl,
                InstagramUrl = b.InstagramUrl,
                CreatedAt = b.CreatedAt,
                ProductCount = b.Products.Count,
                WebsiteCount = b.Websites.Count
            })
            .ToListAsync();

        return Ok(businesses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBusiness(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var business = await _context.Businesses
            .Where(b => b.Id == id && b.UserId == userId)
            .Select(b => new BusinessDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Industry = b.Industry,
                Website = b.Website,
                Logo = b.Logo,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                PhoneNumber = b.PhoneNumber,
                Email = b.Email,
                FacebookUrl = b.FacebookUrl,
                TwitterUrl = b.TwitterUrl,
                TikTokUrl = b.TikTokUrl,
                InstagramUrl = b.InstagramUrl,
                CreatedAt = b.CreatedAt,
                ProductCount = b.Products.Count,
                WebsiteCount = b.Websites.Count
            })
            .FirstOrDefaultAsync();

        if (business == null)
            return NotFound();

        return Ok(business);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var business = new Business
        {
            Name = request.Name,
            Description = request.Description,
            Industry = request.Industry,
            Website = request.Website,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            UserId = userId
        };

        _context.Businesses.Add(business);
        await _context.SaveChangesAsync();

        var businessDto = new BusinessDto
        {
            Id = business.Id,
            Name = business.Name,
            Description = business.Description,
            Industry = business.Industry,
            Website = business.Website,
            Address = business.Address,
            City = business.City,
            Country = business.Country,
            PhoneNumber = business.PhoneNumber,
            Email = business.Email,
            CreatedAt = business.CreatedAt,
            ProductCount = 0,
            WebsiteCount = 0
        };

        return CreatedAtAction(nameof(GetBusiness), new { id = business.Id }, businessDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBusiness(Guid id, [FromBody] UpdateBusinessRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (business == null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Name)) business.Name = request.Name;
        if (request.Description != null) business.Description = request.Description;
        if (request.Industry != null) business.Industry = request.Industry;
        if (request.Website != null) business.Website = request.Website;
        if (request.Logo != null) business.Logo = request.Logo;
        if (request.Address != null) business.Address = request.Address;
        if (request.City != null) business.City = request.City;
        if (request.Country != null) business.Country = request.Country;
        if (request.PhoneNumber != null) business.PhoneNumber = request.PhoneNumber;
        if (request.Email != null) business.Email = request.Email;
        if (request.FacebookUrl != null) business.FacebookUrl = request.FacebookUrl;
        if (request.TwitterUrl != null) business.TwitterUrl = request.TwitterUrl;
        if (request.TikTokUrl != null) business.TikTokUrl = request.TikTokUrl;
        if (request.InstagramUrl != null) business.InstagramUrl = request.InstagramUrl;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBusiness(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (business == null)
            return NotFound();

        _context.Businesses.Remove(business);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}