namespace GestionCommerciale.Application.Orders;

public record OrderOperationResult(
    OrderOperationStatus Status,
    OrderDto? Order = null,
    string? Message = null);
