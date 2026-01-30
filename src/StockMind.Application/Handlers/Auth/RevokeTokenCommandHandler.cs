using StockMind.Application.Commands.Auth;
using StockMind.Application.Common;
using StockMind.Application.Interfaces;

namespace StockMind.Application.Handlers.Auth;

public class RevokeTokenCommandHandler : ICommandHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly IAuthService _authService;

    public RevokeTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RevokeTokenAsync(request.RefreshToken, cancellationToken);
    }
}
