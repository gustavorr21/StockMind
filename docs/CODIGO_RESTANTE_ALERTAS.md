# ?? CÓDIGO RESTANTE DO SISTEMA DE ALERTAS - APLICAR MANUALMENTE

## 4. API LAYER

### 4.1 SignalR Hub
**Arquivo:** `src/StockMind.API/Hubs/StockAlertHub.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StockMind.API.Hubs;

[Authorize]
public class StockAlertHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
```

### 4.2 Stock Alerts Controller
**Arquivo:** `src/StockMind.API/Controllers/StockAlertsController.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMind.Application.Commands.Alerts;
using StockMind.Application.Queries.Alerts;

namespace StockMind.API.Controllers;

[ApiController]
[Route("api/stock-alerts")]
[Authorize]
public class StockAlertsController : BaseController
{
    private readonly IMediator _mediator;

    public StockAlertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Operator,Viewer")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var query = new GetActiveAlertsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("{id}/acknowledge")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AcknowledgeAlert(
        Guid id,
        [FromBody] AcknowledgeAlertRequest request)
    {
        var command = new AcknowledgeAlertCommand(id, request.Notes);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Alert acknowledged successfully" });
    }
}

public record AcknowledgeAlertRequest(string? Notes);
```

## 5. CONFIGURAÇÕES

### 5.1 Atualizar ApplicationDbContext
**Arquivo:** `src/StockMind.Infrastructure/Persistence/ApplicationDbContext.cs`

Adicionar após a linha `public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();`:

```csharp
public DbSet<StockAlert> StockAlerts => Set<StockAlert>();
```

### 5.2 Registrar Serviços
**Arquivo:** `src/StockMind.Infrastructure/Extensions/ServiceCollectionExtensions.cs`

Adicionar após `services.AddScoped<IInventoryRepository, InventoryRepository>();`:

```csharp
// Stock Alerts Repository
services.AddScoped<IStockAlertRepository, StockAlertRepository>();

// Email Service
services.AddScoped<IEmailService, EmailService>();
```

### 5.3 Configurar SignalR
**Arquivo:** `src/StockMind.API/Program.cs`

Adicionar após `builder.Services.AddAuthorization();`:

```csharp
// SignalR
builder.Services.AddSignalR();
```

Adicionar após `app.MapControllers();`:

```csharp
app.MapHub<StockAlertHub>("/hubs/stock-alerts");
```

### 5.4 appsettings.json
**Arquivo:** `src/StockMind.API/appsettings.json`

Adicionar:

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "seu-email@gmail.com",
  "SmtpPassword": "sua-senha-app",
  "FromEmail": "noreply@stockmind.com",
  "AlertsEmail": "alertas@stockmind.com"
}
```

## 6. DISPARAR EVENTO NOS HANDLERS

### 6.1 StockExitCommandHandler
**Arquivo:** `src/StockMind.Application/Handlers/Stock/StockExitCommandHandler.cs`

Adicionar APÓS `stock.RemoveQuantity(request.Quantity);`:

```csharp
// Verificar se o estoque ficou abaixo do mínimo após a saída
if (stock.Product != null && stock.CurrentQuantity <= stock.Product.MinimumStock)
{
    var lowStockEvent = new Domain.Events.LowStockDetectedEvent(
        stock.ProductId,
        stock.Product.Name,
        stock.Product.Sku,
        stock.WarehouseId,
        stock.Warehouse.Name,
        stock.CurrentQuantity,
        stock.Product.MinimumStock,
        stock.AvailableQuantity
    );

    stock.AddDomainEvent(lowStockEvent);
}
```

### 6.2 StockEntryCommandHandler
**Arquivo:** `src/StockMind.Application/Handlers/Stock/StockEntryCommandHandler.cs`

Adicionar APÓS `stock.AddQuantity(request.Quantity);`:

```csharp
// Verificar se o estoque ainda está abaixo do mínimo após a entrada
if (stock.Product != null && stock.CurrentQuantity <= stock.Product.MinimumStock)
{
    var lowStockEvent = new Domain.Events.LowStockDetectedEvent(
        stock.ProductId,
        stock.Product.Name,
        stock.Product.Sku,
        stock.WarehouseId,
        stock.Warehouse.Name,
        stock.CurrentQuantity,
        stock.Product.MinimumStock,
        stock.AvailableQuantity
    );

    stock.AddDomainEvent(lowStockEvent);
}
```

### 6.3 Publicar Domain Events no UnitOfWork
**Arquivo:** `src/StockMind.Infrastructure/Persistence/UnitOfWork.cs`

Substituir o método `SaveChangesAsync` por:

```csharp
public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // Coletar Domain Events
    var domainEvents = _context.ChangeTracker
        .Entries<AggregateRoot>()
        .Where(e => e.Entity.DomainEvents.Any())
        .SelectMany(e => e.Entity.DomainEvents)
        .ToList();

    // Limpar eventos
    foreach (var entry in _context.ChangeTracker.Entries<AggregateRoot>())
    {
        entry.Entity.ClearDomainEvents();
    }

    // Salvar alterações
    var result = await _context.SaveChangesAsync(cancellationToken);

    // Publicar eventos APÓS commit
    foreach (var domainEvent in domainEvents)
    {
        await _mediator.Publish(domainEvent, cancellationToken);
    }

    return result;
}
```

## 7. MIGRATION

Execute no terminal:

```sh
dotnet ef migrations add AddStockAlerts -s src/StockMind.API -p src/StockMind.Infrastructure

dotnet ef database update -s src/StockMind.API -p src/StockMind.Infrastructure
```

## 8. TESTAR

### Criar saída que deixe abaixo do mínimo:
```sh
POST /api/stock/exit
{
  "productId": "guid",
  "warehouseId": "guid",
  "quantity": 50,
  "origin": "Sale"
}
```

### Verificar alertas:
```sh
GET /api/stock-alerts/active
```

---

**NOTA:** O RabbitMQ consumer foi omitido desta implementação inicial para simplificar. O sistema funciona com Domain Events via MediatR. Para adicionar RabbitMQ posteriormente, siga a documentação fornecida anteriormente.
