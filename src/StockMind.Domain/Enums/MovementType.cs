namespace StockMind.Domain.Enums;

/// <summary>
/// Tipo de movimentação de estoque
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Entrada de produtos no estoque
    /// </summary>
    Entry = 1,
    
    /// <summary>
    /// Saída de produtos do estoque
    /// </summary>
    Exit = 2,
    
    /// <summary>
    /// Ajuste de estoque (inventário)
    /// </summary>
    Adjustment = 3,
    
    /// <summary>
    /// Transferência entre depósitos
    /// </summary>
    Transfer = 4
}
