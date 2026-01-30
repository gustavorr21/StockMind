using FluentValidation;
using StockMind.Application.Commands.Auth;
using StockMind.Domain.Enums;

namespace StockMind.Application.Validators.Auth;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("Invalid role. Must be Admin, Manager, Operator, or Viewer");
    }
}
