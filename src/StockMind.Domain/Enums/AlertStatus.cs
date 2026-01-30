namespace StockMind.Domain.Enums;

public enum AlertStatus
{
    Active = 1,      // Alerta ativo, aguardando ação
    Acknowledged = 2, // Alerta reconhecido pelo usuário
    Resolved = 3     // Estoque normalizado automaticamente
}
