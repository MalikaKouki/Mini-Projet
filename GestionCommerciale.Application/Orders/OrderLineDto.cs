namespace GestionCommerciale.Application.Orders;

public record OrderLineDto(
    int ProductId,
    string ProductReference,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
