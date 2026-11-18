using System.ComponentModel.DataAnnotations;

namespace GestionCommerciale.Application.Orders;

public class OrderWriteRequest
{
    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderLineWriteRequest> Lines { get; set; } = new();
}
