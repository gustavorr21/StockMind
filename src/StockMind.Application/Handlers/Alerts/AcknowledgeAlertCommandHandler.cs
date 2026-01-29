using StockMind.Application.Commands.Alerts;
using StockMind.Application.Common;
using StockMind.Application.Interfaces;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Alerts;

public class AcknowledgeAlertCommandHandler : ICommandHandler<AcknowledgeAlertCommand, Result<bool>>
{
    private readonly IStockAlertRepository _alertRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AcknowledgeAlertCommandHandler(
        IStockAlertRepository alertRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _alertRepository = alertRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        AcknowledgeAlertCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            var alert = await _alertRepository.GetByIdAsync(request.AlertId, cancellationToken);
            if (alert == null)
            {
                return Result<bool>.Failure("Alert not found");
            }

            alert.Acknowledge(userId.Value, request.Notes);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error acknowledging alert: {ex.Message}");
        }
    }
}
