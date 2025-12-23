namespace Application.Dtos.Categories;

public record CategorySummaryResponse(
    int CategoryId,
    string CategoryName,
    int TotalProducts,
    int ActiveProducts,
    int OutOfStockCount,
    decimal AveragePrice,
    decimal TotalInventoryValue,
    decimal MinPrice,
    decimal MaxPrice
);