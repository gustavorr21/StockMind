using StockMind.Application.Common;
using StockMind.Application.DTOs.Auth;

namespace StockMind.Application.Commands.Auth;

public sealed record LoginCommand(
    string Email,
    string Password
) : ICommand<Result<AuthResponseDto>>;
