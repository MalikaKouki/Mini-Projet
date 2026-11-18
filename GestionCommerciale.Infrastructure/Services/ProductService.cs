using GestionCommerciale.Application.Products;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly CommercialDbContext _dbContext;

    public ProductService(CommercialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);

        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

        return product is null ? null : ToDto(product);
    }

    public async Task<ProductWriteResult> CreateAsync(
        ProductWriteRequest request,
        CancellationToken cancellationToken)
    {
        var reference = request.Reference.Trim();
        var referenceExists = await _dbContext.Products
            .AnyAsync(product => product.Reference == reference, cancellationToken);

        if (referenceExists)
        {
            return new ProductWriteResult(ProductWriteStatus.ReferenceConflict, null);
        }

        var product = new Product();
        ApplyRequest(product, request, reference);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProductWriteResult(ProductWriteStatus.Success, ToDto(product));
    }

    public async Task<ProductWriteResult> UpdateAsync(
        int id,
        ProductWriteRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

        if (product is null)
        {
            return new ProductWriteResult(ProductWriteStatus.NotFound, null);
        }

        var reference = request.Reference.Trim();
        var referenceExists = await _dbContext.Products
            .AnyAsync(
                existing => existing.Id != id && existing.Reference == reference,
                cancellationToken);

        if (referenceExists)
        {
            return new ProductWriteResult(ProductWriteStatus.ReferenceConflict, null);
        }

        ApplyRequest(product, request, reference);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProductWriteResult(ProductWriteStatus.Success, ToDto(product));
    }

    public async Task<ProductDeleteResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

        if (product is null)
        {
            return ProductDeleteResult.NotFound;
        }

        var hasOrderLines = await _dbContext.OrderLines
            .AnyAsync(line => line.ProductId == id, cancellationToken);

        if (hasOrderLines)
        {
            return ProductDeleteResult.HasOrderLines;
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductDeleteResult.Deleted;
    }

    private static void ApplyRequest(
        Product product,
        ProductWriteRequest request,
        string reference)
    {
        product.Reference = reference;
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.UnitPrice = request.UnitPrice;
        product.StockQuantity = request.StockQuantity;
    }

    private static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Reference,
            product.Name,
            product.Description,
            product.UnitPrice,
            product.StockQuantity,
            product.CreatedAt);
    }
}
