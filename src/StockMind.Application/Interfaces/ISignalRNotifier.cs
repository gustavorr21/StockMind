namespace StockMind.Application.Interfaces;

/// <summary>
/// Interface para notificações via SignalR
/// </summary>
public interface ISignalRNotifier
{
    /// <summary>
    /// Notifica todos os clientes conectados sobre alerta de estoque baixo
    /// </summary>
    Task NotifyLowStockAsync(object alertData);
}
