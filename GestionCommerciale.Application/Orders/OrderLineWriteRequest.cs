using System.ComponentModel.DataAnnotations;

namespace GestionCommerciale.Application.Orders;

public class OrderLineWriteRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
