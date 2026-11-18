namespace GestionCommerciale.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "Brouillon";
    public decimal TotalHT { get; set; }
    public decimal TotalTTC { get; set; }

    public Client Client { get; set; } = null!;
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}
