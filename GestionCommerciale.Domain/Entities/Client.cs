namespace GestionCommerciale.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
