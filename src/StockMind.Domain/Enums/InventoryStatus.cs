namespace StockMind.Domain.Enums;

/// <summary>
/// Status do inventário físico
/// </summary>
public enum InventoryStatus
{
    /// <summary>
    /// Inventário aberto, em andamento
    /// </summary>
    InProgress = 1,
    
    /// <summary>
    /// Inventário finalizado, aguardando aprovação
    /// </summary>
    PendingApproval = 2,
    
    /// <summary>
    /// Inventário aprovado e ajustes aplicados
    /// </summary>
    Completed = 3,
    
    /// <summary>
    /// Inventário cancelado
    /// </summary>
    Cancelled = 4
}
