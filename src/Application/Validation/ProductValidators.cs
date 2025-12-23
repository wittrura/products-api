namespace Application.Validation;

using Application.Dtos.Products;

public static class ProductValidators
{
    public static ValidationResult Validate(ProductCreateRequest req)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(req.Name))
            result.Errors.Add("Name is required.");

        if (req.Price <= 0)
            result.Errors.Add("Price must be greater than 0.");

        if (req.StockQuantity < 0)
            result.Errors.Add("StockQuantity cannot be negative.");

        if (req.CategoryId <= 0)
            result.Errors.Add("CategoryId must be a valid id.");

        return result;
    }

    public static ValidationResult Validate(ProductUpdateRequest req)
    {
        // Same rules for now
        return Validate(new ProductCreateRequest(req.Name, req.Description, req.Price, req.CategoryId, req.StockQuantity));
    }
}