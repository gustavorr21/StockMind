using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StockMind.Application.Common;
using StockMind.Application.DTOs.Auth;
using StockMind.Application.Interfaces;
using StockMind.Domain.Entities;
using StockMind.Infrastructure.Identity;
using StockMind.Infrastructure.Persistence;

namespace StockMind.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<AuthResponseDto>.Failure("Invalid email or password");
            }

            if (!user.IsActive)
            {
                return Result<AuthResponseDto>.Failure("User account is inactive");
            }

            // Check password manually
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                // Increment failed access attempts
                await _userManager.AccessFailedAsync(user);
                
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Result<AuthResponseDto>.Failure("Account is locked. Try again later.");
                }
                
                return Result<AuthResponseDto>.Failure("Invalid email or password");
            }

            // Reset failed access count on successful login
            await _userManager.ResetAccessFailedCountAsync(user);

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate tokens
            return await GenerateAuthResponse(user, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure($"Login failed: {ex.Message}");
        }
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(string email, string password, string fullName, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return Result<AuthResponseDto>.Failure("Email is already registered");
            }

            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FullName = fullName,
                EmailConfirmed = true // Auto-confirm for now
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result<AuthResponseDto>.Failure($"Registration failed: {errors}");
            }

            // Add default role "Viewer" for new users
            var addRoleResult = await _userManager.AddToRoleAsync(user, StockMind.Domain.Enums.UserRole.Viewer.ToString());
            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user); // Rollback
                return Result<AuthResponseDto>.Failure("Failed to assign default role");
            }

            // Generate tokens
            return await GenerateAuthResponse(user, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure($"Registration failed: {ex.Message}");
        }
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                return Result<AuthResponseDto>.Failure("Invalid or expired refresh token");
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null || !user.IsActive)
            {
                return Result<AuthResponseDto>.Failure("User not found or inactive");
            }

            // Revoke old token
            storedToken.Revoke();

            // Generate new tokens
            var response = await GenerateAuthResponse(user, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure($"Token refresh failed: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

            if (storedToken == null)
            {
                return Result<bool>.Failure("Token not found");
            }

            storedToken.Revoke();
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Token revocation failed: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Roles = roles
            };

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to get user: {ex.Message}");
        }
    }

    private async Task<Result<AuthResponseDto>> GenerateAuthResponse(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_configuration["JwtSettings:RefreshTokenExpirationInDays"]!));
        var refreshToken = RefreshToken.Create(refreshTokenString, refreshTokenExpiry, user.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddHours(double.Parse(_configuration["JwtSettings:ExpirationInHours"]!)),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Roles = roles
            }
        };

        return Result<AuthResponseDto>.Success(response);
    }
}
