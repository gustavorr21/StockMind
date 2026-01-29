using StockMind.Domain.Common;
using StockMind.Domain.Enums;
using StockMind.Domain.Events;

namespace StockMind.Domain.Entities;

/// <summary>
/// Entidade Movimentação de Estoque - FONTE DA VERDADE
/// Todo movimento de estoque DEVE criar um registro aqui
/// Esta tabela é APPEND-ONLY (nunca deve ser alterada após criação)
/// O saldo de estoque é calculado pela soma das movimentações
/// </summary>
public sealed class StockMovement : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public MovementType Type { get; private set; }
    public MovementOrigin Origin { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal PreviousBalance { get; private set; }
    public decimal NewBalance { get; private set; }
    public Guid UserId { get; private set; } // Usuário que realizou a movimentação
    public DateTime MovementDate { get; private set; }
    public string? Observation { get; private set; }
    
    // Referências opcionais dependendo da origem
    public Guid? PurchaseOrderId { get; private set; }
    public Guid? PurchaseEntryId { get; private set; }
    public Guid? InventoryId { get; private set; }
    public Guid? TransferDestinationWarehouseId { get; private set; }
    
    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;
    public Warehouse? TransferDestinationWarehouse { get; private set; }
    public PurchaseOrder? PurchaseOrder { get; private set; }
    public PurchaseEntry? PurchaseEntry { get; private set; }
    public Inventory? Inventory { get; private set; }

    // EF Core constructor
    private StockMovement()
    {
    }

    private StockMovement(
        Guid productId,
        Guid warehouseId,
        MovementType type,
        MovementOrigin origin,
        decimal quantity,
        decimal previousBalance,
        Guid userId,
        string? observation)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        Type = type;
        Origin = origin;
        Quantity = quantity;
        PreviousBalance = previousBalance;
        NewBalance = CalculateNewBalance(type, previousBalance, quantity);
        UserId = userId;
        MovementDate = DateTime.UtcNow;
        Observation = observation;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;

        // Dispara evento de domínio
        AddDomainEvent(new StockMovedEvent(Id, ProductId, WarehouseId, Type, Origin, Quantity, NewBalance));
    }

    /// <summary>
    /// Cria uma entrada de estoque (compra, devolução, etc)
    /// </summary>
    public static StockMovement CreateEntry(
        Guid productId,
        Guid warehouseId,
        MovementOrigin origin,
        decimal quantity,
        decimal previousBalance,
        Guid userId,
        string? observation = null)
    {
        ValidateBasicParameters(productId, warehouseId, quantity, userId);

        if (origin != MovementOrigin.Purchase && 
            origin != MovementOrigin.Return && 
            origin != MovementOrigin.ManualAdjustment)
        {
            throw new ArgumentException("Invalid origin for entry movement", nameof(origin));
        }

        return new StockMovement(
            productId,
            warehouseId,
            MovementType.Entry,
            origin,
            quantity,
            previousBalance,
            userId,
            observation);
    }

    /// <summary>
    /// Cria uma saída de estoque (venda, perda, etc)
    /// </summary>
    public static StockMovement CreateExit(
        Guid productId,
        Guid warehouseId,
        MovementOrigin origin,
        decimal quantity,
        decimal previousBalance,
        Guid userId,
        string? observation = null)
    {
        ValidateBasicParameters(productId, warehouseId, quantity, userId);

        if (origin != MovementOrigin.Sale && 
            origin != MovementOrigin.Loss && 
            origin != MovementOrigin.ManualAdjustment)
        {
            throw new ArgumentException("Invalid origin for exit movement", nameof(origin));
        }

        // Valida se há saldo suficiente
        var newBalance = previousBalance - quantity;
        if (newBalance < 0)
        {
            throw new InvalidOperationException(
                $"Insufficient stock. Current: {previousBalance}, Requested: {quantity}");
        }

        return new StockMovement(
            productId,
            warehouseId,
            MovementType.Exit,
            origin,
            quantity,
            previousBalance,
            userId,
            observation);
    }

    /// <summary>
    /// Cria um ajuste de estoque (via inventário)
    /// </summary>
    public static StockMovement CreateAdjustment(
        Guid productId,
        Guid warehouseId,
        decimal quantityDifference,
        decimal previousBalance,
        Guid userId,
        Guid inventoryId,
        string? observation = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID cannot be empty", nameof(warehouseId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (inventoryId == Guid.Empty)
            throw new ArgumentException("Inventory ID cannot be empty", nameof(inventoryId));

        var movement = new StockMovement(
            productId,
            warehouseId,
            MovementType.Adjustment,
            MovementOrigin.Inventory,
            Math.Abs(quantityDifference),
            previousBalance,
            userId,
            observation);

        movement.InventoryId = inventoryId;
        
        return movement;
    }

    /// <summary>
    /// Cria uma transferência entre depósitos
    /// </summary>
    public static StockMovement CreateTransfer(
        Guid productId,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        decimal quantity,
        decimal previousBalance,
        Guid userId,
        string? observation = null)
    {
        ValidateBasicParameters(productId, sourceWarehouseId, quantity, userId);

        if (destinationWarehouseId == Guid.Empty)
            throw new ArgumentException("Destination warehouse ID cannot be empty", nameof(destinationWarehouseId));

        if (sourceWarehouseId == destinationWarehouseId)
            throw new ArgumentException("Source and destination warehouses cannot be the same");

        // Valida se há saldo suficiente
        var newBalance = previousBalance - quantity;
        if (newBalance < 0)
        {
            throw new InvalidOperationException(
                $"Insufficient stock for transfer. Current: {previousBalance}, Requested: {quantity}");
        }

        var movement = new StockMovement(
            productId,
            sourceWarehouseId,
            MovementType.Transfer,
            MovementOrigin.Transfer,
            quantity,
            previousBalance,
            userId,
            observation);

        movement.TransferDestinationWarehouseId = destinationWarehouseId;

        return movement;
    }

    /// <summary>
    /// Associa esta movimentação a um pedido de compra
    /// </summary>
    public void AssociatePurchaseOrder(Guid purchaseOrderId, Guid purchaseEntryId)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("Purchase order ID cannot be empty", nameof(purchaseOrderId));

        if (purchaseEntryId == Guid.Empty)
            throw new ArgumentException("Purchase entry ID cannot be empty", nameof(purchaseEntryId));

        PurchaseOrderId = purchaseOrderId;
        PurchaseEntryId = purchaseEntryId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static decimal CalculateNewBalance(MovementType type, decimal previousBalance, decimal quantity)
    {
        return type switch
        {
            MovementType.Entry => previousBalance + quantity,
            MovementType.Exit => previousBalance - quantity,
            MovementType.Transfer => previousBalance - quantity,
            MovementType.Adjustment => previousBalance + quantity, // quantity já vem com sinal correto
            _ => throw new ArgumentException("Invalid movement type", nameof(type))
        };
    }

    private static void ValidateBasicParameters(Guid productId, Guid warehouseId, decimal quantity, Guid userId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID cannot be empty", nameof(warehouseId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }
}
