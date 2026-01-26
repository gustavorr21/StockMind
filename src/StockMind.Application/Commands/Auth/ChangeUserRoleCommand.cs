using StockMind.Application.Common;

namespace StockMind.Application.Commands.Auth;

public sealed record ChangeUserRoleCommand(
    Guid UserId,
    string NewRole
) : ICommand<Result<bool>>;
