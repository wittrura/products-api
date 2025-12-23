using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(b =>
        {
            b.ToTable("Categories");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(2000);

            b.HasMany(x => x.Products)
             .WithOne(x => x.Category)
             .HasForeignKey(x => x.CategoryId);

            b.HasIndex(x => x.IsActive);
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(2000);

            b.Property(x => x.Price)
             .HasColumnType("decimal(18,2)");

            b.Property(x => x.CreatedDate)
             .HasDefaultValueSql("CURRENT_TIMESTAMP"); // only valid for SQLite

            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => new { x.CategoryId, x.IsActive });
        });
    }
}