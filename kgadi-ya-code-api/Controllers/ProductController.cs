using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using kgadi_ya_code_api.Data;
using kgadi_ya_code_api.Models;
using kgadi_ya_code_api.DTOs;
using kgadi_ya_code_api.Services;
using System.Security.Claims;

namespace kgadi_ya_code_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAIService _aiService;

    public ProductController(ApplicationDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetProducts(Guid businessId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Verify business ownership
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        var products = await _context.Products
            .Where(p => p.BusinessId == businessId)
            .Include(p => p.Business)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                AiGeneratedDescription = p.AiGeneratedDescription,
                Price = p.Price,
                Category = p.Category,
                SKU = p.SKU,
                StockQuantity = p.StockQuantity,
                Images = p.Images,
                Tags = p.Tags,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                BusinessId = p.BusinessId,
                BusinessName = p.Business.Name
            })
            .ToListAsync();

        var totalCount = await _context.Products.CountAsync(p => p.BusinessId == businessId);

        return Ok(new
        {
            products,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var product = await _context.Products
            .Include(p => p.Business)
            .Where(p => p.Id == id && p.Business.UserId == userId)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                AiGeneratedDescription = p.AiGeneratedDescription,
                Price = p.Price,
                Category = p.Category,
                SKU = p.SKU,
                StockQuantity = p.StockQuantity,
                Images = p.Images,
                Tags = p.Tags,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                BusinessId = p.BusinessId,
                BusinessName = p.Business.Name
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Verify business ownership
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == request.BusinessId && b.UserId == userId);
        
        if (business == null)
            return NotFound("Business not found");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            SKU = request.SKU,
            StockQuantity = request.StockQuantity,
            Images = request.Images,
            Tags = request.Tags,
            BusinessId = request.BusinessId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            SKU = product.SKU,
            StockQuantity = product.StockQuantity,
            Images = product.Images,
            Tags = product.Tags,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            BusinessId = product.BusinessId,
            BusinessName = business.Name
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var product = await _context.Products
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.Id == id && p.Business.UserId == userId);

        if (product == null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Name)) product.Name = request.Name;
        if (request.Description != null) product.Description = request.Description;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.Category != null) product.Category = request.Category;
        if (request.SKU != null) product.SKU = request.SKU;
        if (request.StockQuantity.HasValue) product.StockQuantity = request.StockQuantity.Value;
        if (request.Images != null) product.Images = request.Images;
        if (request.Tags != null) product.Tags = request.Tags;
        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
        
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var product = await _context.Products
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.Id == id && p.Business.UserId == userId);

        if (product == null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("generate-description")]
    public async Task<IActionResult> GenerateDescription([FromBody] GenerateDescriptionRequest request)
    {
        try
        {
            var description = await _aiService.GenerateProductDescriptionAsync(request);
            return Ok(new { description });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to generate description", error = ex.Message });
        }
    }

    [HttpPost("{id}/generate-description")]
    public async Task<IActionResult> GenerateDescriptionForProduct(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var product = await _context.Products
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.Id == id && p.Business.UserId == userId);

        if (product == null)
            return NotFound();

        try
        {
            var request = new GenerateDescriptionRequest
            {
                ProductName = product.Name,
                Category = product.Category,
                Features = product.Tags,
                Tone = "professional"
            };

            var description = await _aiService.GenerateProductDescriptionAsync(request);
            
            product.AiGeneratedDescription = description;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { description });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to generate description", error = ex.Message });
        }
    }

    [HttpPost("suggest-tags")]
    public async Task<IActionResult> SuggestTags([FromBody] SuggestTagsRequest request)
    {
        try
        {
            var tags = await _aiService.SuggestProductTagsAsync(request.ProductName, request.Category ?? "");
            return Ok(new { tags });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to suggest tags", error = ex.Message });
        }
    }
}

public class SuggestTagsRequest
{
    public string ProductName { get; set; } = "";
    public string? Category { get; set; }
}