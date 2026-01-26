using StockMind.Application.Commands.Auth;
using StockMind.Application.Common;
using StockMind.Application.DTOs.Auth;
using StockMind.Application.Interfaces;

namespace StockMind.Application.Handlers.Auth;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
    }
}
