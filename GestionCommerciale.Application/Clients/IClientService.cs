namespace GestionCommerciale.Application.Clients;

public interface IClientService
{
    Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ClientDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ClientDto> CreateAsync(
        ClientWriteRequest request,
        CancellationToken cancellationToken);
    Task<ClientDto?> UpdateAsync(
        int id,
        ClientWriteRequest request,
        CancellationToken cancellationToken);
    Task<ClientDeleteResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
