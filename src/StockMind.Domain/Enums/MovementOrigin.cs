namespace StockMind.Domain.Enums;

/// <summary>
/// Origem da movimentação de estoque
/// </summary>
public enum MovementOrigin
{
    /// <summary>
    /// Entrada via compra de fornecedor
    /// </summary>
    Purchase = 1,
    
    /// <summary>
    /// Saída via venda
    /// </summary>
    Sale = 2,
    
    /// <summary>
    /// Ajuste via inventário físico
    /// </summary>
    Inventory = 3,
    
    /// <summary>
    /// Devolução de cliente
    /// </summary>
    Return = 4,
    
    /// <summary>
    /// Perda ou avaria
    /// </summary>
    Loss = 5,
    
    /// <summary>
    /// Transferência entre depósitos
    /// </summary>
    Transfer = 6,
    
    /// <summary>
    /// Ajuste manual
    /// </summary>
    ManualAdjustment = 7
}
