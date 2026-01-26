using Microsoft.AspNetCore.Identity;
using StockMind.Application.Commands.Auth;
using StockMind.Application.Common;
using StockMind.Infrastructure.Identity;

namespace StockMind.Infrastructure.Services;

public class ChangeUserRoleCommandHandler : ICommandHandler<ChangeUserRoleCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangeUserRoleCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // Validate role
            string[] validRoles = { "Admin", "Manager", "Operator", "Viewer" };
            if (!validRoles.Contains(request.NewRole))
            {
                return Result<bool>.Failure($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
            }

            // Remove all existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return Result<bool>.Failure("Failed to remove current roles");
                }
            }

            // Add new role
            var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
            if (!addResult.Succeeded)
            {
                return Result<bool>.Failure("Failed to assign new role");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to change user role: {ex.Message}");
        }
    }
}
