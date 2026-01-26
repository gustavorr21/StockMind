using StockMind.Application.Common;
using StockMind.Application.DTOs.Auth;

namespace StockMind.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RegisterAsync(string email, string password, string fullName, string role, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<bool>> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
