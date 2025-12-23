namespace Application.Dtos.Products;

public record ProductUpdateRequest(
    string Name,
    string? Description,
    decimal Price,
    int CategoryId,
    int StockQuantity
);