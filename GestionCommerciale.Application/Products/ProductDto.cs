namespace GestionCommerciale.Application.Products;

public record ProductDto(
    int Id,
    string Reference,
    string Name,
    string? Description,
    decimal UnitPrice,
    int StockQuantity,
    DateTime CreatedAt);
