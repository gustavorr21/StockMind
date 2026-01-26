using StockMind.Domain.Common;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Entities;

public sealed class StockMovement : BaseEntity
{
    public Guid ProductId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    public int Quantity { get; private set; }
    public int PreviousQuantity { get; private set; }
    public int NewQuantity { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public Guid? UserId { get; private set; }

    // Navigation property
    public Product? Product { get; private set; }

    private StockMovement(Guid productId, StockMovementType movementType, int quantity, int previousQuantity, int newQuantity, string? reference, string? notes, Guid? userId)
    {
        ProductId = productId;
        MovementType = movementType;
        Quantity = quantity;
        PreviousQuantity = previousQuantity;
        NewQuantity = newQuantity;
        Reference = reference;
        Notes = notes;
        UserId = userId;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static StockMovement Create(
        Guid productId,
        StockMovementType movementType,
        int quantity,
        int previousQuantity,
        int newQuantity,
        string? reference = null,
        string? notes = null,
        Guid? userId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (previousQuantity < 0)
            throw new ArgumentException("Previous quantity cannot be negative", nameof(previousQuantity));

        if (newQuantity < 0)
            throw new ArgumentException("New quantity cannot be negative", nameof(newQuantity));

        return new StockMovement(productId, movementType, quantity, previousQuantity, newQuantity, reference?.Trim(), notes?.Trim(), userId);
    }

    public void AddNotes(string notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
