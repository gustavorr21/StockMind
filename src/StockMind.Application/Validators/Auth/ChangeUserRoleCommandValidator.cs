using FluentValidation;
using StockMind.Application.Commands.Auth;

namespace StockMind.Application.Validators.Auth;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.NewRole)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => new[] { "Admin", "Manager", "Operator", "Viewer" }.Contains(role))
            .WithMessage("Role must be one of: Admin, Manager, Operator, Viewer");
    }
}
