using GestionCommerciale.Application.Clients;
using Microsoft.AspNetCore.Mvc;

namespace GestionCommerciale.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var clients = await _clientService.GetAllAsync(cancellationToken);
        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.GetByIdAsync(id, cancellationToken);
        if (client is null)
        {
            return NotFound();
        }

        return Ok(client);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Create(
        ClientWriteRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> Update(
        int id,
        ClientWriteRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.UpdateAsync(id, request, cancellationToken);
        if (client is null)
        {
            return NotFound();
        }

        return Ok(client);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.DeleteAsync(id, cancellationToken);

        switch (result)
        {
            case ClientDeleteResult.Deleted:
                return NoContent();
            case ClientDeleteResult.NotFound:
                return NotFound();
            case ClientDeleteResult.HasOrders:
                return Conflict(
                    new { message = "Un client ayant des commandes ne peut pas être supprimé." });
            default:
                throw new InvalidOperationException("Résultat de suppression inconnu.");
        }
    }
}
