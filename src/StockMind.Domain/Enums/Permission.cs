namespace StockMind.Domain.Enums;

public enum Permission
{
    // Dashboard
    DashboardView = 1,

    // Products
    ProductsView = 10,
    ProductsCreate = 11,
    ProductsUpdate = 12,
    ProductsDelete = 13,

    // Stock
    StockView = 20,
    StockAdd = 21,
    StockRemove = 22,
    StockAdjust = 23,

    // Categories
    CategoriesView = 30,
    CategoriesCreate = 31,
    CategoriesUpdate = 32,

    // Reports
    ReportsView = 40,
    ReportsLowStock = 41,
    ReportsMovements = 42,

    // Administration
    UsersView = 50,
    UsersCreate = 51,
    UsersUpdate = 52,
    UsersDelete = 53,
    RolesManage = 54
}
