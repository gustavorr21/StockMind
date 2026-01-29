namespace StockMind.Application.Interfaces;

public interface IEmailService
{
    Task SendLowStockAlertEmailAsync(
        string productName,
        string productSku,
        string warehouseName,
        decimal currentQuantity,
        decimal minimumQuantity,
        CancellationToken cancellationToken = default);
}
