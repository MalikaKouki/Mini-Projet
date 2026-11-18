namespace GestionCommerciale.Application.Orders;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OrderOperationResult> CreateAsync(
        OrderWriteRequest request,
        CancellationToken cancellationToken);
    Task<OrderOperationResult> UpdateAsync(
        int id,
        OrderWriteRequest request,
        CancellationToken cancellationToken);
    Task<OrderOperationStatus> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<OrderOperationResult> ValidateAsync(int id, CancellationToken cancellationToken);
}
