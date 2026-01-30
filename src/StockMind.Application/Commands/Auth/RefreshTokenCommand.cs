using StockMind.Application.Common;
using StockMind.Application.DTOs.Auth;

namespace StockMind.Application.Commands.Auth;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : ICommand<Result<AuthResponseDto>>;
