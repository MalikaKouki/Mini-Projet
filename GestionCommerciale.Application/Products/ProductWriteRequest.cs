using System.ComponentModel.DataAnnotations;

namespace GestionCommerciale.Application.Products;

public class ProductWriteRequest
{
    [Required]
    [MaxLength(50)]
    public string Reference { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true)]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
