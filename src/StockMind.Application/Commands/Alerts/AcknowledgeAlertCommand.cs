using StockMind.Application.Common;

namespace StockMind.Application.Commands.Alerts;

public sealed record AcknowledgeAlertCommand(
    Guid AlertId,
    string? Notes
) : ICommand<Result<bool>>;
