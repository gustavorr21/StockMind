using StockMind.Application.Common;
using StockMind.Application.DTOs.Alerts;

namespace StockMind.Application.Queries.Alerts;

public sealed record GetActiveAlertsQuery : IQuery<Result<List<StockAlertDto>>>;
