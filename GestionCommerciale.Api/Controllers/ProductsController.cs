using GestionCommerciale.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace GestionCommerciale.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Create(
        ProductWriteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(request, cancellationToken);

        if (result.Status == ProductWriteStatus.ReferenceConflict)
        {
            return Conflict(new { message = "Cette référence produit existe déjà." });
        }

        var product = result.Product
            ?? throw new InvalidOperationException("Le produit créé est introuvable.");

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Update(
        int id,
        ProductWriteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, request, cancellationToken);

        if (result.Status == ProductWriteStatus.NotFound)
        {
            return NotFound();
        }

        if (result.Status == ProductWriteStatus.ReferenceConflict)
        {
            return Conflict(new { message = "Cette référence produit existe déjà." });
        }

        var product = result.Product
            ?? throw new InvalidOperationException("Le produit modifié est introuvable.");

        return Ok(product);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(id, cancellationToken);

        switch (result)
        {
            case ProductDeleteResult.Deleted:
                return NoContent();
            case ProductDeleteResult.NotFound:
                return NotFound();
            case ProductDeleteResult.HasOrderLines:
                return Conflict(
                    new { message = "Un produit présent dans une commande ne peut pas être supprimé." });
            default:
                throw new InvalidOperationException("Résultat de suppression inconnu.");
        }
    }
}
