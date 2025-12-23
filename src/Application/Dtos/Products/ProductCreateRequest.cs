namespace Application.Dtos.Products;

public record ProductCreateRequest(
    string Name,
    string? Description,
    decimal Price,
    int CategoryId,
    int StockQuantity
);