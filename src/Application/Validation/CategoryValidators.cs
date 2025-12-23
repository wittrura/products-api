namespace Application.Validation;

using Application.Dtos.Categories;

public static class CategoryValidators
{
    public static ValidationResult Validate(CategoryCreateRequest req)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(req.Name))
            result.Errors.Add("Name is required.");

        return result;
    }
}