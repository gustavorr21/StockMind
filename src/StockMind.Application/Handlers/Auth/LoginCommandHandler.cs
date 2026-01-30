using StockMind.Application.Commands.Auth;
using StockMind.Application.Common;
using StockMind.Application.DTOs.Auth;
using StockMind.Application.Interfaces;

namespace StockMind.Application.Handlers.Auth;

public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}
