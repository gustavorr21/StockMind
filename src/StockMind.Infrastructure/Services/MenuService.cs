using StockMind.Application.DTOs.Menu;
using StockMind.Application.Interfaces;
using StockMind.Domain.Enums;

namespace StockMind.Infrastructure.Services;

public class MenuService : IMenuService
{
    private static readonly Dictionary<UserRole, IEnumerable<Permission>> RolePermissions = new()
    {
        {
            UserRole.Admin, new[]
            {
                // Dashboard
                Permission.DashboardView,
                // Products - Full Access
                Permission.ProductsView, Permission.ProductsCreate, Permission.ProductsUpdate, Permission.ProductsDelete,
                // Stock - Full Access
                Permission.StockView, Permission.StockAdd, Permission.StockRemove, Permission.StockAdjust,
                // Categories - Full Access
                Permission.CategoriesView, Permission.CategoriesCreate, Permission.CategoriesUpdate,
                // Reports - Full Access
                Permission.ReportsView, Permission.ReportsLowStock, Permission.ReportsMovements,
                // Administration - Full Access
                Permission.UsersView, Permission.UsersCreate, Permission.UsersUpdate, Permission.UsersDelete, Permission.RolesManage
            }
        },
        {
            UserRole.Manager, new[]
            {
                // Dashboard
                Permission.DashboardView,
                // Products - Full Access
                Permission.ProductsView, Permission.ProductsCreate, Permission.ProductsUpdate, Permission.ProductsDelete,
                // Stock - Full Access
                Permission.StockView, Permission.StockAdd, Permission.StockRemove, Permission.StockAdjust,
                // Categories - Full Access
                Permission.CategoriesView, Permission.CategoriesCreate, Permission.CategoriesUpdate,
                // Reports - Full Access
                Permission.ReportsView, Permission.ReportsLowStock, Permission.ReportsMovements
            }
        },
        {
            UserRole.Operator, new[]
            {
                // Dashboard
                Permission.DashboardView,
                // Products - View Only
                Permission.ProductsView,
                // Stock - Add/Remove Only
                Permission.StockView, Permission.StockAdd, Permission.StockRemove,
                // Categories - View Only
                Permission.CategoriesView,
                // Reports - View Only
                Permission.ReportsView, Permission.ReportsLowStock
            }
        },
        {
            UserRole.Viewer, new[]
            {
                // Dashboard
                Permission.DashboardView,
                // Products - View Only
                Permission.ProductsView,
                // Stock - View Only
                Permission.StockView,
                // Categories - View Only
                Permission.CategoriesView,
                // Reports - View Only
                Permission.ReportsView, Permission.ReportsLowStock
            }
        }
    };

    public MenuResponse GetMenuForUser(IEnumerable<string> userRoles)
    {
        var permissions = GetPermissionsForRoles(userRoles).ToHashSet();
        var sections = GetAllMenuSections();

        // Filter sections and items based on permissions
        var filteredSections = sections
            .Where(section => HasRequiredPermissions(section.RequiredPermissions, permissions))
            .Select(section => section with
            {
                Items = section.Items?
                    .Where(item => HasRequiredPermissions(item.RequiredPermissions, permissions))
                    .OrderBy(item => item.Order)
            })
            .OrderBy(section => section.Order)
            .ToList();

        return new MenuResponse { Sections = filteredSections };
    }

    public IEnumerable<Permission> GetPermissionsForRoles(IEnumerable<string> userRoles)
    {
        var permissions = new HashSet<Permission>();

        foreach (var roleName in userRoles)
        {
            if (Enum.TryParse<UserRole>(roleName, out var role))
            {
                if (RolePermissions.TryGetValue(role, out var rolePermissions))
                {
                    foreach (var permission in rolePermissions)
                    {
                        permissions.Add(permission);
                    }
                }
            }
        }

        return permissions;
    }

    private static bool HasRequiredPermissions(IEnumerable<Permission>? required, HashSet<Permission> userPermissions)
    {
        if (required == null || !required.Any())
            return true;

        return required.Any(p => userPermissions.Contains(p));
    }

    private static IEnumerable<MenuSection> GetAllMenuSections()
    {
        return new List<MenuSection>
        {
            // Dashboard
            new MenuSection
            {
                Id = "dashboard",
                Title = "Dashboard",
                Icon = "dashboard",
                Route = "/dashboard",
                Order = 1,
                RequiredPermissions = new[] { Permission.DashboardView }
            },

            // Products
            new MenuSection
            {
                Id = "products",
                Title = "Products",
                Icon = "inventory_2",
                Order = 2,
                RequiredPermissions = new[] { Permission.ProductsView },
                Items = new[]
                {
                    new MenuItem
                    {
                        Id = "products-list",
                        Title = "All Products",
                        Icon = "list",
                        Route = "/products",
                        Order = 1,
                        RequiredPermissions = new[] { Permission.ProductsView }
                    },
                    new MenuItem
                    {
                        Id = "products-create",
                        Title = "Add Product",
                        Icon = "add_circle",
                        Route = "/products/create",
                        Order = 2,
                        RequiredPermissions = new[] { Permission.ProductsCreate }
                    },
                    new MenuItem
                    {
                        Id = "categories",
                        Title = "Categories",
                        Icon = "category",
                        Route = "/categories",
                        Order = 3,
                        RequiredPermissions = new[] { Permission.CategoriesView }
                    }
                }
            },

            // Stock
            new MenuSection
            {
                Id = "stock",
                Title = "Stock",
                Icon = "warehouse",
                Order = 3,
                RequiredPermissions = new[] { Permission.StockView },
                Items = new[]
                {
                    new MenuItem
                    {
                        Id = "stock-overview",
                        Title = "Stock Overview",
                        Icon = "inventory",
                        Route = "/stock",
                        Order = 1,
                        RequiredPermissions = new[] { Permission.StockView }
                    },
                    new MenuItem
                    {
                        Id = "stock-add",
                        Title = "Add Stock",
                        Icon = "add_box",
                        Route = "/stock/add",
                        Order = 2,
                        RequiredPermissions = new[] { Permission.StockAdd }
                    },
                    new MenuItem
                    {
                        Id = "stock-remove",
                        Title = "Remove Stock",
                        Icon = "remove_circle",
                        Route = "/stock/remove",
                        Order = 3,
                        RequiredPermissions = new[] { Permission.StockRemove }
                    },
                    new MenuItem
                    {
                        Id = "stock-adjust",
                        Title = "Adjust Stock",
                        Icon = "tune",
                        Route = "/stock/adjust",
                        Order = 4,
                        RequiredPermissions = new[] { Permission.StockAdjust }
                    },
                    new MenuItem
                    {
                        Id = "stock-movements",
                        Title = "Stock Movements",
                        Icon = "swap_horiz",
                        Route = "/stock/movements",
                        Order = 5,
                        RequiredPermissions = new[] { Permission.ReportsMovements }
                    }
                }
            },

            // Reports
            new MenuSection
            {
                Id = "reports",
                Title = "Reports",
                Icon = "assessment",
                Order = 4,
                RequiredPermissions = new[] { Permission.ReportsView },
                Items = new[]
                {
                    new MenuItem
                    {
                        Id = "reports-low-stock",
                        Title = "Low Stock Alert",
                        Icon = "warning",
                        Route = "/reports/low-stock",
                        Order = 1,
                        RequiredPermissions = new[] { Permission.ReportsLowStock }
                    },
                    new MenuItem
                    {
                        Id = "reports-movements",
                        Title = "Stock Movements",
                        Icon = "timeline",
                        Route = "/reports/movements",
                        Order = 2,
                        RequiredPermissions = new[] { Permission.ReportsMovements }
                    }
                }
            },

            // Administration
            new MenuSection
            {
                Id = "administration",
                Title = "Administration",
                Icon = "admin_panel_settings",
                Order = 5,
                RequiredPermissions = new[] { Permission.UsersView, Permission.RolesManage },
                Items = new[]
                {
                    new MenuItem
                    {
                        Id = "admin-users",
                        Title = "Users",
                        Icon = "people",
                        Route = "/admin/users",
                        Order = 1,
                        RequiredPermissions = new[] { Permission.UsersView }
                    },
                    new MenuItem
                    {
                        Id = "admin-roles",
                        Title = "Roles & Permissions",
                        Icon = "security",
                        Route = "/admin/roles",
                        Order = 2,
                        RequiredPermissions = new[] { Permission.RolesManage }
                    }
                }
            }
        };
    }
}
