using StockMind.Domain.Common;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Events;

/// <summary>
/// Evento disparado quando há movimentação de estoque
/// Usado para atualizar cache, notificar sistemas, gerar alertas
/// </summary>
public sealed record StockMovedEvent : IDomainEvent
{
    public Guid MovementId { get; init; }
    public Guid ProductId { get; init; }
    public Guid WarehouseId { get; init; }
    public MovementType MovementType { get; init; }
    public MovementOrigin Origin { get; init; }
    public decimal Quantity { get; init; }
    public decimal NewBalance { get; init; }
    public DateTime OccurredOn { get; init; }

    public StockMovedEvent(
        Guid movementId,
        Guid productId,
        Guid warehouseId,
        MovementType movementType,
        MovementOrigin origin,
        decimal quantity,
        decimal newBalance)
    {
        MovementId = movementId;
        ProductId = productId;
        WarehouseId = warehouseId;
        MovementType = movementType;
        Origin = origin;
        Quantity = quantity;
        NewBalance = newBalance;
        OccurredOn = DateTime.UtcNow;
    }
}
