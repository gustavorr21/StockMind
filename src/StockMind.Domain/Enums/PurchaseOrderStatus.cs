namespace StockMind.Domain.Enums;

/// <summary>
/// Status do pedido de compra
/// </summary>
public enum PurchaseOrderStatus
{
    /// <summary>
    /// Pedido criado, aguardando confirmação
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Pedido confirmado pelo fornecedor
    /// </summary>
    Confirmed = 2,
    
    /// <summary>
    /// Produtos em trânsito
    /// </summary>
    InTransit = 3,
    
    /// <summary>
    /// Pedido recebido parcialmente
    /// </summary>
    PartiallyReceived = 4,
    
    /// <summary>
    /// Pedido recebido completamente
    /// </summary>
    Received = 5,
    
    /// <summary>
    /// Pedido cancelado
    /// </summary>
    Cancelled = 6
}
