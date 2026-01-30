using MediatR;
using StockMind.Application.Common;
using StockMind.Application.DTOs.Alerts;
using StockMind.Domain.Enums;

namespace StockMind.Application.Queries.Alerts;

public class GetStockAlertsQuery : IRequest<Result<PagedResult<StockAlertDto>>>
{
    public AlertStatus? Status { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
