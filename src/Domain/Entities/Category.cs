namespace Domain.Entities;

public class Category
{
    public int Id { get; set; } // PK
    public string Name { get; set; } = string.Empty; // required
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Product> Products { get; set; } = [];
}