using StockMind.Application.Common;
using StockMind.Application.DTOs.Auth;

namespace StockMind.Application.Commands.Auth;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FullName
) : ICommand<Result<AuthResponseDto>>;
