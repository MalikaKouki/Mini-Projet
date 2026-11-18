using GestionCommerciale.Application.Clients;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly CommercialDbContext _dbContext;

    public ClientService(CommercialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ClientDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var clients = await _dbContext.Clients
            .AsNoTracking()
            .OrderBy(client => client.Id)
            .ToListAsync(cancellationToken);

        return clients.Select(ToDto).ToList();
    }

    public async Task<ClientDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var client = await _dbContext.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(client => client.Id == id, cancellationToken);

        return client is null ? null : ToDto(client);
    }

    public async Task<ClientDto> CreateAsync(
        ClientWriteRequest request,
        CancellationToken cancellationToken)
    {
        var client = new Client();
        ApplyRequest(client, request);

        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(client);
    }

    public async Task<ClientDto?> UpdateAsync(
        int id,
        ClientWriteRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _dbContext.Clients
            .FirstOrDefaultAsync(client => client.Id == id, cancellationToken);

        if (client is null)
        {
            return null;
        }

        ApplyRequest(client, request);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(client);
    }

    public async Task<ClientDeleteResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var client = await _dbContext.Clients
            .FirstOrDefaultAsync(client => client.Id == id, cancellationToken);

        if (client is null)
        {
            return ClientDeleteResult.NotFound;
        }

        var hasOrders = await _dbContext.Orders
            .AnyAsync(order => order.ClientId == id, cancellationToken);

        if (hasOrders)
        {
            return ClientDeleteResult.HasOrders;
        }

        _dbContext.Clients.Remove(client);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ClientDeleteResult.Deleted;
    }

    private static void ApplyRequest(Client client, ClientWriteRequest request)
    {
        client.LastName = request.LastName.Trim();
        client.FirstName = request.FirstName?.Trim();
        client.Email = request.Email?.Trim();
        client.Phone = request.Phone?.Trim();
        client.Address = request.Address?.Trim();
    }

    private static ClientDto ToDto(Client client)
    {
        return new ClientDto(
            client.Id,
            client.LastName,
            client.FirstName,
            client.Email,
            client.Phone,
            client.Address,
            client.CreatedAt);
    }
}
