namespace GestionCommerciale.Application.Orders;

public record OrderDto(
    int Id,
    string OrderNumber,
    int ClientId,
    string ClientName,
    DateTime OrderDate,
    string Status,
    decimal TotalHT,
    decimal TotalTTC,
    IReadOnlyList<OrderLineDto> Lines);
