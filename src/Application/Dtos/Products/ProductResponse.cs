namespace Application.Dtos.Products;

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedDate,
    int CategoryId,
    string CategoryName
);