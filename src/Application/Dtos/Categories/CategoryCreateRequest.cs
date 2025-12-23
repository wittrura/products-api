namespace Application.Dtos.Categories;

public record CategoryCreateRequest(
    string Name,
    string? Description
);