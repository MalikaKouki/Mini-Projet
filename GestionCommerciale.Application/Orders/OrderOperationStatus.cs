namespace GestionCommerciale.Application.Orders;

public enum OrderOperationStatus
{
    Success,
    NotFound,
    ClientNotFound,
    ProductNotFound,
    InsufficientStock,
    DuplicateProducts,
    InvalidState,
    EmptyOrder
}
