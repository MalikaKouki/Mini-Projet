namespace GestionCommerciale.Application.Clients;

public record ClientDto(
    int Id,
    string LastName,
    string? FirstName,
    string? Email,
    string? Phone,
    string? Address,
    DateTime CreatedAt);
