namespace StockMind.Application.DTOs.Auth;

public sealed record RegisterRequestDto(
    string Email,
    string Password,
    string FullName
);
