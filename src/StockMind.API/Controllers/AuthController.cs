using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMind.Application.Commands.Auth;
using StockMind.Application.DTOs.Auth;
using StockMind.Domain.Enums;
using System.Security.Claims;

namespace StockMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FullName);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Revoke/logout - invalidate refresh token
    /// </summary>
    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto request)
    {
        var command = new RevokeTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Token revoked successfully" });
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                     User.FindFirst("userId")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst("fullName")?.Value;
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);

        return Ok(new
        {
            userId,
            email,
            fullName,
            roles
        });
    }

    /// <summary>
    /// Change user role (Admin only)
    /// </summary>
    [HttpPut("users/{userId:guid}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeUserRole(Guid userId, [FromBody] ChangeUserRoleRequest request)
    {
        var command = new ChangeUserRoleCommand(userId, request.NewRole);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "User role updated successfully" });
    }
}

public record ChangeUserRoleRequest(UserRole NewRole);
