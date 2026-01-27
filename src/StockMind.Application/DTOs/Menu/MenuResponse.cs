using StockMind.Domain.Enums;

namespace StockMind.Application.DTOs.Menu;

public record MenuResponse
{
    public IEnumerable<MenuSection> Sections { get; init; } = new List<MenuSection>();
}

public record MenuSection
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string? Route { get; init; }
    public int Order { get; init; }
    public IEnumerable<MenuItem>? Items { get; init; }
    public IEnumerable<Permission>? RequiredPermissions { get; init; }
}

public record MenuItem
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public int Order { get; init; }
    public IEnumerable<Permission> RequiredPermissions { get; init; } = new List<Permission>();
    public bool Divider { get; init; } = false;
}
