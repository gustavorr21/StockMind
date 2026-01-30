using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMind.Application.DTOs.Menu;
using StockMind.Application.Interfaces;
using System.Security.Claims;

namespace StockMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : BaseController
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    /// <summary>
    /// Get menu items and permissions for the current authenticated user
    /// </summary>
    /// <returns>Menu structure with sections and items based on user roles</returns>
    [HttpGet("user-menu")]
    public IActionResult GetUserMenu()
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (!roles.Any())
        {
            return Ok(new MenuResponse { Sections = Array.Empty<MenuSection>() });
        }

        var menu = _menuService.GetMenuForUser(roles);
        return Ok(menu);
    }

    /// <summary>
    /// Get all permissions for the current authenticated user
    /// </summary>
    /// <returns>List of permissions</returns>
    [HttpGet("user-permissions")]
    public IActionResult GetUserPermissions()
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var permissions = _menuService.GetPermissionsForRoles(roles);

        return Ok(new
        {
            permissions = permissions.Select(p => new
            {
                id = (int)p,
                name = p.ToString()
            })
        });
    }
}
