namespace Adashop.DTOs;

public record ChangeUserDetailsRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Address
);