using Application.Dtos.Categories;
using Application.Dtos.Products;
using Application.Validation;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/categories", async (AppDbContext db) =>
{
    var categories = await db.Categories
        .AsNoTracking()
        .Where(c => c.IsActive)
        .OrderBy(c => c.Name)
        .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
        .ToListAsync();

    return Results.Ok(categories);
});

app.MapPost("/api/categories", async (AppDbContext db, CategoryCreateRequest request) =>
{
    var validation = CategoryValidators.Validate(request);
    if (!validation.IsValid)
        return Results.BadRequest(new { errors = validation.Errors });

    var entity = new Category
    {
        Name = request.Name.Trim(),
        Description = request.Description?.Trim(),
        IsActive = true
    };

    db.Categories.Add(entity);
    await db.SaveChangesAsync();

    var response = new CategoryResponse(entity.Id, entity.Name, entity.Description);
    return Results.Created($"/api/categories/{entity.Id}", response);
});

app.MapGet("/api/products", async (AppDbContext db) =>
{
    var products = await db.Products
        .AsNoTracking()
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity,
            p.CreatedDate,
            p.CategoryId,
            p.Category!.Name
        ))
        .ToListAsync();

    return Results.Ok(products);
});

app.MapGet("/api/products/{id:int}", async (AppDbContext db, int id) =>
{
    var product = await db.Products
        .AsNoTracking()
        .Where(p => p.IsActive && p.Id == id)
        .Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity,
            p.CreatedDate,
            p.CategoryId,
            p.Category!.Name
        ))
        .SingleOrDefaultAsync();

    return product is null
        ? Results.NotFound()
        : Results.Ok(product);
});

app.MapPost("/api/products", async (AppDbContext db, ProductCreateRequest request) =>
{
    var validation = ProductValidators.Validate(request);
    if (!validation.IsValid)
        return Results.BadRequest(new { errors = validation.Errors });

    // Validate category exists and is active
    var category = await db.Categories
        .AsNoTracking()
        .Where(c => c.Id == request.CategoryId && c.IsActive)
        .Select(c => new { c.Id, c.Name })
        .SingleOrDefaultAsync();

    if (category is null)
        return Results.BadRequest(new { errors = new[] { "CategoryId must reference an active category." } });

    var entity = new Product
    {
        Name = request.Name.Trim(),
        Description = request.Description?.Trim(),
        Price = request.Price,
        CategoryId = category.Id,
        StockQuantity = request.StockQuantity,
        CreatedDate = DateTime.UtcNow,
        IsActive = true
    };

    db.Products.Add(entity);
    await db.SaveChangesAsync();

    var response = new ProductResponse(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.Price,
        entity.StockQuantity,
        entity.CreatedDate,
        entity.CategoryId,
        category.Name
    );

    return Results.Created($"/api/products/{entity.Id}", response);
});

app.MapPut("/api/products/{id:int}", async (AppDbContext db, int id, ProductUpdateRequest request) =>
{
    var validation = ProductValidators.Validate(request);
    if (!validation.IsValid)
        return Results.BadRequest(new { errors = validation.Errors });

    // Ensure target product exists and is active - tracked, because we will modify it
    var product = await db.Products
        .Where(p => p.Id == id && p.IsActive)
        .SingleOrDefaultAsync();

    if (product is null)
        return Results.NotFound();

    // Validate category exists and is active
    var category = await db.Categories
        .AsNoTracking()
        .Where(c => c.Id == request.CategoryId && c.IsActive)
        .Select(c => new { c.Id, c.Name })
        .SingleOrDefaultAsync();

    if (category is null)
        return Results.BadRequest(new { errors = new[] { "CategoryId must reference an active category." } });

    // Apply updates
    product.Name = request.Name.Trim();
    product.Description = request.Description?.Trim();
    product.Price = request.Price;
    product.CategoryId = category.Id;
    product.StockQuantity = request.StockQuantity;

    await db.SaveChangesAsync();

    var response = new ProductResponse(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.CreatedDate,
        product.CategoryId,
        category.Name
    );

    return Results.Ok(response);
});

app.MapDelete("/api/products/{id:int}", async (AppDbContext db, int id) =>
{
    var product = await db.Products
        .Where(p => p.Id == id && p.IsActive)
        .SingleOrDefaultAsync();

    if (product is null)
        return Results.NotFound();

    product.IsActive = false;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapGet("/api/categories/{id:int}/summary", async (AppDbContext db, int id) =>
{
    // Verify category exists
    var category = await db.Categories
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new { c.Id, c.Name, c.IsActive })
        .SingleOrDefaultAsync();

    if (category is null)
        return Results.Problem(title: "Not found.", statusCode: StatusCodes.Status404NotFound);

    // Currently we summarize products even if category is inactive
    // To consider: if we want to treat inactive as 404 to customers
    // if (!category.IsActive)
    //     return Results.NotFound();

    // Aggregate over products
    var agg = await db.Products
        .AsNoTracking()
        .Where(p => p.CategoryId == id)
        .GroupBy(_ => 1)
        .Select(g => new
        {
            TotalProducts = g.Count(),
            ActiveProducts = g.Count(p => p.IsActive),
            OutOfStockCount = g.Count(p => p.IsActive && p.StockQuantity == 0),

            AveragePrice = g.Where(p => p.IsActive).Average(p => (decimal?)p.Price) ?? 0m,
            TotalInventoryValue = g.Where(p => p.IsActive).Sum(p => (decimal?)(p.Price * p.StockQuantity)) ?? 0m,

            MinPrice = g.Where(p => p.IsActive).Min(p => (decimal?)p.Price) ?? 0m,
            MaxPrice = g.Where(p => p.IsActive).Max(p => (decimal?)p.Price) ?? 0m
        })
        .SingleOrDefaultAsync();


    // If category has zero products, agg will be null because GroupBy produces no rows - default to zeros
    var response = new CategorySummaryResponse(
        CategoryId: category.Id,
        CategoryName: category.Name,
        TotalProducts: agg?.TotalProducts ?? 0,
        ActiveProducts: agg?.ActiveProducts ?? 0,
        OutOfStockCount: agg?.OutOfStockCount ?? 0,
        AveragePrice: agg?.AveragePrice ?? 0m,
        TotalInventoryValue: agg?.TotalInventoryValue ?? 0m,
        MinPrice: agg?.MinPrice ?? 0m,
        MaxPrice: agg?.MaxPrice ?? 0m
    );

    return Results.Ok(response);
});

app.Run();