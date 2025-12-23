namespace Application.Dtos.Categories;

public record CategoryResponse(
    int Id,
    string Name,
    string? Description
);