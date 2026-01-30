using StockMind.Application.Common;
using StockMind.Application.DTOs.Alerts;
using StockMind.Application.Queries.Alerts;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Alerts;

public class GetActiveAlertsQueryHandler : IQueryHandler<GetActiveAlertsQuery, Result<List<StockAlertDto>>>
{
    private readonly IStockAlertRepository _alertRepository;

    public GetActiveAlertsQueryHandler(IStockAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task<Result<List<StockAlertDto>>> Handle(
        GetActiveAlertsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var alerts = await _alertRepository.GetActiveAlertsAsync(cancellationToken);

            var dtos = alerts.Select(a => new StockAlertDto
            {
                Id = a.Id,
                ProductId = a.ProductId,
                ProductName = a.ProductName,
                ProductSku = a.ProductSku,
                WarehouseId = a.WarehouseId,
                WarehouseName = a.WarehouseName,
                CurrentQuantity = a.CurrentQuantity,
                MinimumQuantity = a.MinimumQuantity,
                Status = a.Status.ToString(),
                FirstDetectedAt = a.FirstDetectedAt,
                LastNotifiedAt = a.LastNotifiedAt,
                ResolvedAt = a.ResolvedAt,
                AcknowledgedAt = a.AcknowledgedAt,
                Notes = a.Notes
            }).ToList();

            return Result<List<StockAlertDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<StockAlertDto>>.Failure($"Error getting active alerts: {ex.Message}");
        }
    }
}
