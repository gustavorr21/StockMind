using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockMind.Domain.Entities;
using StockMind.Infrastructure.Persistence;

namespace StockMind.Infrastructure.Data;

public static class StockSystemSeeder
{
    public static async Task SeedStockSystemAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Warehouse if not exists
        if (!await context.Warehouses.AnyAsync())
        {
            var mainWarehouse = Warehouse.Create(
                "Depósito Principal",
                "Depósito principal da empresa",
                "Endereço do depósito principal",
                isMain: true
            );

            var secondaryWarehouse = Warehouse.Create(
                "Depósito Secundário",
                "Depósito para overflow",
                "Endereço do depósito secundário",
                isMain: false
            );

            await context.Warehouses.AddAsync(mainWarehouse);
            await context.Warehouses.AddAsync(secondaryWarehouse);
            await context.SaveChangesAsync();
        }

        // Seed Categories if not exists
        if (!await context.Categories.AnyAsync())
        {
            var categoryEletronics = Category.Create("Eletrônicos", "Produtos eletrônicos");
            var categoryComputers = Category.Create("Computadores", "Notebooks, desktops, periféricos", categoryEletronics.Id);
            var categoryPhones = Category.Create("Celulares", "Smartphones e acessórios", categoryEletronics.Id);
            
            var categoryFurniture = Category.Create("Móveis", "Móveis para escritório");
            var categoryOfficeChairs = Category.Create("Cadeiras", "Cadeiras de escritório", categoryFurniture.Id);

            await context.Categories.AddRangeAsync(
                categoryEletronics,
                categoryComputers,
                categoryPhones,
                categoryFurniture,
                categoryOfficeChairs
            );
            await context.SaveChangesAsync();
        }
    }
}
