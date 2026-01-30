using StockMind.Application.DTOs.Menu;
using StockMind.Domain.Enums;

namespace StockMind.Application.Interfaces;

public interface IMenuService
{
    MenuResponse GetMenuForUser(IEnumerable<string> userRoles);
    IEnumerable<Permission> GetPermissionsForRoles(IEnumerable<string> userRoles);
}
