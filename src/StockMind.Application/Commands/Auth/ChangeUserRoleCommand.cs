using StockMind.Application.Common;
using StockMind.Domain.Enums;

namespace StockMind.Application.Commands.Auth;

public sealed record ChangeUserRoleCommand(
    Guid UserId,
    UserRole NewRole
) : ICommand<Result<bool>>;
