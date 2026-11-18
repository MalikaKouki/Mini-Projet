namespace GestionCommerciale.Application.Products;

public record ProductWriteResult(ProductWriteStatus Status, ProductDto? Product);
