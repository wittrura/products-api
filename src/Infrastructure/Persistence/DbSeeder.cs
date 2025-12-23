using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // If we already have categories, assume seeded.
        if (await db.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Electronics", Description = "Gadgets and devices", IsActive = true },
            new() { Name = "Home",        Description = "Home essentials",     IsActive = true },
            new() { Name = "Fitness",     Description = "Fitness gear",        IsActive = true },
            new() { Name = "Books",       Description = "Books & media",       IsActive = true },
            new() { Name = "Legacy",      Description = "Inactive category",   IsActive = false },
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        // Reload to ensure IDs are set
        var electronics = await db.Categories.SingleAsync(c => c.Name == "Electronics");
        var home        = await db.Categories.SingleAsync(c => c.Name == "Home");
        var fitness     = await db.Categories.SingleAsync(c => c.Name == "Fitness");
        var books       = await db.Categories.SingleAsync(c => c.Name == "Books");
        var legacy      = await db.Categories.SingleAsync(c => c.Name == "Legacy");

        var now = DateTime.UtcNow;

        var products = new List<Product>
        {
            new() { Name="Wireless Headphones", Description="Over-ear", Price=149.99m, CategoryId=electronics.Id, StockQuantity=25, CreatedDate=now.AddDays(-10), IsActive=true },
            new() { Name="USB-C Charger",       Description="65W",     Price=39.99m,  CategoryId=electronics.Id, StockQuantity=0,  CreatedDate=now.AddDays(-7),  IsActive=true },
            new() { Name="Smart Light Bulb",    Description="Color",   Price=19.99m,  CategoryId=electronics.Id, StockQuantity=80, CreatedDate=now.AddDays(-20), IsActive=true },

            new() { Name="Chef Knife",          Description="8-inch",  Price=59.99m,  CategoryId=home.Id,        StockQuantity=12, CreatedDate=now.AddDays(-15), IsActive=true },
            new() { Name="Cast Iron Skillet",   Description="12-inch", Price=34.99m,  CategoryId=home.Id,        StockQuantity=5,  CreatedDate=now.AddDays(-30), IsActive=true },
            new() { Name="Cutting block",       Description="Wood",    Price=50.99m,  CategoryId=home.Id,        StockQuantity=10,  CreatedDate=now.AddDays(-30), IsActive=true },

            new() { Name="Yoga Mat",            Description="Non-slip",Price=29.99m,  CategoryId=fitness.Id,     StockQuantity=40, CreatedDate=now.AddDays(-5),  IsActive=true },
            new() { Name="Kettlebell 35lb",     Description="Cast",    Price=69.99m,  CategoryId=fitness.Id,     StockQuantity=8,  CreatedDate=now.AddDays(-2),  IsActive=true },
            new() { Name="Resistance Bands",    Description="Set",     Price=14.99m,  CategoryId=fitness.Id,     StockQuantity=0,  CreatedDate=now.AddDays(-1),  IsActive=true },

            new() { Name="Distributed Systems", Description="Textbook",Price=89.00m,  CategoryId=books.Id,       StockQuantity=3,  CreatedDate=now.AddDays(-60), IsActive=true },
            new() { Name="Clean Architecture",  Description="Patterns",Price=42.00m,  CategoryId=books.Id,       StockQuantity=10, CreatedDate=now.AddDays(-45), IsActive=true },
            new() { Name="Refactoring",         Description="Patterns",Price=40.00m,  CategoryId=books.Id,       StockQuantity=5, CreatedDate=now.AddDays(-45), IsActive=true },

            // Inactive product for edge cases
            new() { Name="Old Model Router",     Description="Legacy",  Price=24.99m, CategoryId=electronics.Id, StockQuantity=1,  CreatedDate=now.AddDays(-365),IsActive=false },
            new() { Name="Old Model Laptop",     Description="Legacy",  Price=100.99m, CategoryId=electronics.Id, StockQuantity=1,  CreatedDate=now.AddDays(-365),IsActive=false },

            // Product in inactive category
            new() { Name="Discontinued Item",    Description="Legacy",  Price=9.99m,  CategoryId=legacy.Id,      StockQuantity=10, CreatedDate=now.AddDays(-200),IsActive=true },
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}