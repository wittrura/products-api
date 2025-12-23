namespace Domain.Entities;

public class Product
{
    public int Id { get; set; } // PK
    public string Name { get; set; } = string.Empty; // required
    public string? Description { get; set; }
    public decimal Price { get; set; }  // > 0

    public int CategoryId { get; set; } // FK
    public Category? Category { get; set; }

    public int StockQuantity { get; set; } // >= 0
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true; // soft-delete
}