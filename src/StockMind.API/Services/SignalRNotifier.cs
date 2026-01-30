using Microsoft.AspNetCore.SignalR;
using StockMind.Application.Interfaces;

namespace StockMind.API.Services;

public class SignalRNotifier : ISignalRNotifier
{
    private readonly IHubContext<Hubs.StockAlertHub> _hubContext;
    private readonly ILogger<SignalRNotifier> _logger;

    public SignalRNotifier(
        IHubContext<Hubs.StockAlertHub> hubContext,
        ILogger<SignalRNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyLowStockAsync(object alertData)
    {
        try
        {
            _logger.LogInformation("?? Sending SignalR notification to all clients");
            await _hubContext.Clients.All.SendAsync("LowStockAlert", alertData);
            _logger.LogInformation("? SignalR notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error sending SignalR notification");
            throw;
        }
    }
}
