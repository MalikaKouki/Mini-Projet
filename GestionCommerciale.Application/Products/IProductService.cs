namespace GestionCommerciale.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ProductWriteResult> CreateAsync(
        ProductWriteRequest request,
        CancellationToken cancellationToken);
    Task<ProductWriteResult> UpdateAsync(
        int id,
        ProductWriteRequest request,
        CancellationToken cancellationToken);
    Task<ProductDeleteResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
