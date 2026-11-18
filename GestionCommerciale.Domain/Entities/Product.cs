namespace GestionCommerciale.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}
