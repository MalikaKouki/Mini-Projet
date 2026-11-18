using System.ComponentModel.DataAnnotations;

namespace GestionCommerciale.Application.Clients;

public class ClientWriteRequest
{
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [EmailAddress]
    [MaxLength(254)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }
}
