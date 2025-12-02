using Microsoft.EntityFrameworkCore;
using kgadi_ya_code_api.Models;
using System.Text.Json;

namespace kgadi_ya_code_api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductAnalytics> ProductAnalytics { get; set; }
    public DbSet<Website> Websites { get; set; }
    public DbSet<WebsitePage> WebsitePages { get; set; }
    public DbSet<SocialMediaAnalytics> SocialMediaAnalytics { get; set; }
    public DbSet<SocialMediaPost> SocialMediaPosts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        // Business configuration
        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Businesses)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Images)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                  .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                      (c1, c2) => c1!.SequenceEqual(c2!),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));
            entity.Property(e => e.Tags)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                  .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                      (c1, c2) => c1!.SequenceEqual(c2!),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));
            entity.HasOne(e => e.Business)
                  .WithMany(e => e.Products)
                  .HasForeignKey(e => e.BusinessId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Website configuration
        modelBuilder.Entity<Website>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Config)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<WebsiteConfig>(v, (JsonSerializerOptions?)null) ?? new WebsiteConfig());
            entity.HasOne(e => e.Business)
                  .WithMany(e => e.Websites)
                  .HasForeignKey(e => e.BusinessId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WebsitePage configuration
        modelBuilder.Entity<WebsitePage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.MetaKeywords)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                  .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                      (c1, c2) => c1!.SequenceEqual(c2!),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));
            entity.HasOne(e => e.Website)
                  .WithMany(e => e.Pages)
                  .HasForeignKey(e => e.WebsiteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SocialMediaAnalytics configuration
        modelBuilder.Entity<SocialMediaAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AdditionalMetrics)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            entity.HasOne(e => e.Business)
                  .WithMany(e => e.SocialMediaAnalytics)
                  .HasForeignKey(e => e.BusinessId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SocialMediaPost configuration
        modelBuilder.Entity<SocialMediaPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MediaUrls)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                  .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                      (c1, c2) => c1!.SequenceEqual(c2!),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));
            entity.HasOne(e => e.Business)
                  .WithMany()
                  .HasForeignKey(e => e.BusinessId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductAnalytics configuration
        modelBuilder.Entity<ProductAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConversionRate).HasPrecision(5, 4);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.ProductAnalytics)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}