using System.Security.Claims;

namespace StockMind.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(dynamic user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
