using StockMind.Application.Common;

namespace StockMind.Application.Commands.Auth;

public sealed record RevokeTokenCommand(
    string RefreshToken
) : ICommand<Result<bool>>;
