using GestionCommerciale.Application.Orders;
using Microsoft.AspNetCore.Mvc;

namespace GestionCommerciale.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Create(
        OrderWriteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(request, cancellationToken);

        if (result.Order is not null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Order.Id },
                result.Order);
        }

        return result.Status switch
        {
            OrderOperationStatus.ClientNotFound =>
                BadRequest(new { message = "Le client indiqué n'existe pas." }),
            OrderOperationStatus.ProductNotFound =>
                BadRequest(new { message = "Un ou plusieurs produits n'existent pas." }),
            OrderOperationStatus.DuplicateProducts =>
                BadRequest(new { message = "Chaque produit ne peut apparaître qu'une fois." }),
            OrderOperationStatus.EmptyOrder =>
                BadRequest(new { message = "Une commande doit contenir au moins une ligne." }),
            OrderOperationStatus.InsufficientStock =>
                Conflict(new { message = result.Message }),
            _ => throw new InvalidOperationException("Résultat de création de commande inattendu.")
        };
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Update(
        int id,
        OrderWriteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateAsync(id, request, cancellationToken);

        if (result.Order is not null)
        {
            return Ok(result.Order);
        }

        return result.Status switch
        {
            OrderOperationStatus.NotFound => NotFound(),
            OrderOperationStatus.ClientNotFound =>
                BadRequest(new { message = "Le client indiqué n'existe pas." }),
            OrderOperationStatus.ProductNotFound =>
                BadRequest(new { message = "Un ou plusieurs produits n'existent pas." }),
            OrderOperationStatus.DuplicateProducts =>
                BadRequest(new { message = "Chaque produit ne peut apparaître qu'une fois." }),
            OrderOperationStatus.EmptyOrder =>
                BadRequest(new { message = "Une commande doit contenir au moins une ligne." }),
            OrderOperationStatus.InsufficientStock =>
                Conflict(new { message = result.Message }),
            _ => throw new InvalidOperationException("Résultat de modification de commande inattendu.")
        };
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.DeleteAsync(id, cancellationToken);

        return result switch
        {
            OrderOperationStatus.Success => NoContent(),
            OrderOperationStatus.NotFound => NotFound(),
            _ => throw new InvalidOperationException("Résultat de suppression de commande inattendu.")
        };
    }

    [HttpPost("{id:int}/validate")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Validate(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.ValidateAsync(id, cancellationToken);

        if (result.Order is not null)
        {
            return Ok(result.Order);
        }

        return result.Status switch
        {
            OrderOperationStatus.NotFound => NotFound(),
            OrderOperationStatus.EmptyOrder =>
                BadRequest(new { message = "Une commande vide ne peut pas être validée." }),
            OrderOperationStatus.InsufficientStock =>
                Conflict(new { message = result.Message }),
            OrderOperationStatus.InvalidState =>
                Conflict(new { message = "Seule une commande en brouillon peut être validée." }),
            _ => throw new InvalidOperationException("Résultat de validation de commande inattendu.")
        };
    }

}
