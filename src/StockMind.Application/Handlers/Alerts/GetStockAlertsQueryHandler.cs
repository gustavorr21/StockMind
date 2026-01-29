using MediatR;
using Microsoft.Extensions.Logging;
using StockMind.Application.Common;
using StockMind.Application.DTOs.Alerts;
using StockMind.Application.Queries.Alerts;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Alerts;

public class GetStockAlertsQueryHandler : IRequestHandler<GetStockAlertsQuery, Result<PagedResult<StockAlertDto>>>
{
    private readonly IStockAlertRepository _alertRepository;
    private readonly ILogger<GetStockAlertsQueryHandler> _logger;

    public GetStockAlertsQueryHandler(
        IStockAlertRepository alertRepository,
        ILogger<GetStockAlertsQueryHandler> logger)
    {
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<StockAlertDto>>> Handle(
        GetStockAlertsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Getting stock alerts with filters: Status={Status}, Page={Page}, PageSize={PageSize}",
                request.Status,
                request.Page,
                request.PageSize);

            // Get alerts with filters
            var alerts = await _alertRepository.GetFilteredAlertsAsync(
                request.Status,
                request.ProductId,
                request.WarehouseId,
                request.StartDate,
                request.EndDate,
                cancellationToken);

            // Apply pagination
            var totalCount = alerts.Count;
            var pagedAlerts = alerts
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new StockAlertDto
                {
                    Id = a.Id,
                    ProductId = a.ProductId,
                    ProductName = a.Product.Name,
                    ProductSku = a.Product.Sku,
                    WarehouseId = a.WarehouseId,
                    WarehouseName = a.Warehouse.Name,
                    CurrentQuantity = a.CurrentQuantity,
                    MinimumQuantity = a.MinimumQuantity,
                    Status = a.Status.ToString(),
                    FirstDetectedAt = a.FirstDetectedAt,
                    LastNotifiedAt = a.LastNotifiedAt,
                    ResolvedAt = a.ResolvedAt,
                    AcknowledgedAt = a.AcknowledgedAt,
                    Notes = a.Notes
                })
                .ToList();

            var pagedResult = new PagedResult<StockAlertDto>
            {
                Items = pagedAlerts,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            _logger.LogInformation(
                "Retrieved {Count} alerts (Page {Page} of {TotalPages})",
                pagedAlerts.Count,
                request.Page,
                pagedResult.TotalPages);

            return Result<PagedResult<StockAlertDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock alerts");
            return Result<PagedResult<StockAlertDto>>.Failure("Error getting stock alerts");
        }
    }
}

