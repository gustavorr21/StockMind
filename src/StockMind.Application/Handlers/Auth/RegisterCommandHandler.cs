using StockMind.Application.Commands.Auth;
using StockMind.Application.Common;
using StockMind.Application.DTOs.Auth;
using StockMind.Application.Interfaces;

namespace StockMind.Application.Handlers.Auth;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FullName,
            request.Role,
            cancellationToken
        );
    }
}
