using Microsoft.EntityFrameworkCore;
using SmartShop.Models;

namespace SmartShop.Data;

public class SmartShopDbContext(DbContextOptions<SmartShopDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");
            entity.HasKey(p => p.ProductId);
            entity.Property(p => p.ProductId).ValueGeneratedNever();
            entity.Property(p => p.ProductName).HasMaxLength(128).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(64).IsRequired();
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Features).HasMaxLength(512).IsRequired();
            entity.Property(p => p.Description).HasColumnType("nvarchar(max)");
            entity.HasData(SeedData.Products);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Review", t => t.HasCheckConstraint("CK_Review_Rating", "[Rating] BETWEEN 1 AND 5"));
            entity.HasKey(r => r.ReviewId);
            entity.Property(r => r.ReviewId).UseIdentityColumn();
            entity.Property(r => r.CustomerName).HasMaxLength(64).IsRequired();
            entity.Property(r => r.Content).HasMaxLength(1000).IsRequired();
            entity.Property(r => r.Sentiment).HasMaxLength(16).IsRequired();
            entity.Property(r => r.Summary).HasMaxLength(256).IsRequired();
            entity.Property(r => r.CreatedAt).HasColumnType("datetime");
            entity.HasOne(r => r.Product)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(SeedData.Reviews);
        });
    }
}
