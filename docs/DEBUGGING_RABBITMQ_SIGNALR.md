# ?? GUIA DE DEBUGGING - RabbitMQ + SignalR

## ?? ONDE COLOCAR BREAKPOINTS PARA TESTAR

### **1?? TESTAR PUBLICAÇÃO NO RABBITMQ**

#### **Breakpoint 1: Quando detecta estoque baixo**
?? **Arquivo:** `src/StockMind.Infrastructure/Persistence/Repositories/StockMovementRepository.cs`
?? **Linha:** Dentro do método `RecordMovementAsync`, após criar o `StockAlert`

```csharp
// Criar alerta
var alert = StockAlert.Create(...);
await _dbContext.StockAlerts.AddAsync(alert, cancellationToken);
await _dbContext.SaveChangesAsync(cancellationToken);

// ?? BREAKPOINT AQUI ??
_logger.LogInformation("Low stock detected for product...");
```

#### **Breakpoint 2: Handler que publica evento**
?? **Arquivo:** `src/StockMind.Application/Handlers/Events/LowStockDetectedEventHandler.cs`
?? **Linha 32:** Antes de publicar

```csharp
// ?? BREAKPOINT AQUI ??
await _eventPublisher.PublishAsync(notification, cancellationToken);
```

**O que verificar:**
- ? Variável `notification` contém os dados corretos
- ? `_eventPublisher` é do tipo `RabbitMQEventPublisher` (não `DummyEventPublisher`)

#### **Breakpoint 3: RabbitMQEventPublisher**
?? **Arquivo:** `src/StockMind.Infrastructure/Messaging/RabbitMQ/RabbitMQEventPublisher.cs`
?? **Linha 20:** Dentro do `PublishAsync`

```csharp
_logger.LogInformation("?? Publishing event {EventType} to RabbitMQ queue", typeof(TEvent).Name);

// ?? BREAKPOINT AQUI ??
await _eventBus.PublishAsync(@event, cancellationToken);
```

**O que verificar:**
- ? Evento está sendo serializado corretamente
- ? Conexão com RabbitMQ está ativa

#### **Breakpoint 4: RabbitMQEventBus (publicação)**
?? **Arquivo:** `src/StockMind.Infrastructure/Messaging/RabbitMQ/RabbitMQEventBus.cs`
?? **Linha 19:** Antes de publicar na fila

```csharp
var queueName = typeof(TEvent).Name;
_channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

var message = JsonSerializer.Serialize(@event);
var body = Encoding.UTF8.GetBytes(message);

// ?? BREAKPOINT AQUI ??
_channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
```

**O que verificar:**
- ? `queueName` = "LowStockDetectedEvent"
- ? `message` contém JSON válido
- ? `_channel` não é null

---

### **2?? TESTAR CONSUMO DO RABBITMQ**

#### **Breakpoint 5: Consumer recebe mensagem**
?? **Arquivo:** `src/StockMind.Infrastructure/Messaging/RabbitMQ/RabbitMQConsumerService.cs`
?? **Linha 47:** Quando mensagem é recebida

```csharp
consumer.Received += async (model, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        
        // ?? BREAKPOINT AQUI ??
        _logger.LogInformation("?? Received message from queue {QueueName}", queueName);
```

**O que verificar:**
- ? `message` contém o JSON do evento
- ? Callback está sendo executado

#### **Breakpoint 6: Processamento do evento**
?? **Arquivo:** `src/StockMind.Infrastructure/Messaging/RabbitMQ/RabbitMQConsumerService.cs`
?? **Linha 73:** Antes de enviar via SignalR

```csharp
private async Task ProcessLowStockEvent(LowStockDetectedEvent @event)
{
    try
    {
        // ?? BREAKPOINT AQUI ??
        _logger.LogInformation(
            "?? Processing low stock alert: Product {ProductName}...",
            @event.ProductName);
```

**O que verificar:**
- ? Evento foi deserializado corretamente
- ? Propriedades têm valores corretos

---

### **3?? TESTAR SIGNALR**

#### **Breakpoint 7: Envio via SignalR**
?? **Arquivo:** `src/StockMind.Infrastructure/Messaging/RabbitMQ/RabbitMQConsumerService.cs`
?? **Linha 84:** Antes de enviar notificação

```csharp
// ?? BREAKPOINT AQUI ??
await hubContext.Clients.All.SendAsync("LowStockAlert", new
{
    ProductId = @event.ProductId,
    ProductName = @event.ProductName,
    ...
});
```

**O que verificar:**
- ? `hubContext` não é null
- ? Objeto anônimo contém todos os dados

#### **Breakpoint 8: Hub de SignalR**
?? **Arquivo:** `src/StockMind.API/Hubs/StockAlertHub.cs`
?? **Linha 11:** Quando cliente conecta

```csharp
public override async Task OnConnectedAsync()
{
    // ?? BREAKPOINT AQUI ??
    await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
    await base.OnConnectedAsync();
}
```

**O que verificar:**
- ? `Context.ConnectionId` tem valor
- ? Callback é executado quando frontend conecta

---

## ?? PASSO A PASSO COMPLETO DE TESTE

### **PREPARAÇÃO**
1. ? Subir Docker: `docker-compose up -d`
2. ? Verificar RabbitMQ Management: http://localhost:15672
3. ? Colocar TODOS os breakpoints acima
4. ? Executar API: `dotnet run` (ou F5)

### **TESTAR PUBLICAÇÃO**
1. ?? Fazer login via Swagger
2. ?? Criar entrada de estoque (100 unidades)
3. ?? Criar saída de estoque (55 unidades) - **TRIGGER DO ALERTA**
4. ?? Debugger deve parar em **Breakpoint 1** ? **2** ? **3** ? **4**

### **VERIFICAR RABBITMQ**
1. ?? Ir para RabbitMQ Management (http://localhost:15672)
2. ?? Clicar em **Queues** ? Ver fila `LowStockDetectedEvent`
3. ?? **Verificar:** Message count = 1 (ou mais)

### **TESTAR CONSUMO**
1. ?? Debugger deve continuar e parar em **Breakpoint 5** ? **6** ? **7**
2. ?? Ver log: `?? Received message from queue LowStockDetectedEvent`

### **TESTAR SIGNALR**
1. ?? Conectar frontend ao hub: `https://localhost:7000/hubs/stock-alerts`
2. ?? Criar nova saída de estoque
3. ?? Debugger deve parar em **Breakpoint 8** (conexão) e **Breakpoint 7** (notificação)
4. ?? Frontend deve receber evento `LowStockAlert`

---

## ?? LOGS ESPERADOS

### **Console da API (com RabbitMQ ativo):**
```
? RabbitMQ Event Publisher and Consumer configured successfully
[INF] ?? Starting RabbitMQ Consumer Service...
[INF] ? RabbitMQ Consumer Service started successfully. Listening to queue: LowStockDetectedEvent
[INF] Low stock detected for product Produto X (SKU123) in warehouse Principal. Current: 45, Minimum: 50
[INF] ?? Publishing event LowStockDetectedEvent to RabbitMQ queue
[INF] ? Event LowStockDetectedEvent published successfully to RabbitMQ
[INF] ?? Received message from queue LowStockDetectedEvent
[INF] ?? Processing low stock alert: Product Produto X, Current: 45, Minimum: 50
[INF] ?? SignalR notification sent to all clients for product Produto X
[INF] ? Message processed successfully
```

### **Console da API (com DummyEventPublisher):**
```
?? RabbitMQ:HostName not configured. Using DummyEventPublisher.
[INF] Event LowStockDetectedEvent would be published (dummy implementation)
```

---

## ? TROUBLESHOOTING

### **Breakpoint 3 não para?**
?? Verificar se `_eventPublisher` é `DummyEventPublisher`
?? Verificar `appsettings.json`: `"RabbitMQ": { "HostName": "localhost" }`

### **Breakpoint 5 não para?**
?? RabbitMQ Consumer não está rodando
?? Verificar logs: `Starting RabbitMQ Consumer Service...`
?? Verificar Docker: `docker ps | grep rabbitmq`

### **Breakpoint 7 não para?**
?? Consumer não processou mensagem
?? Verificar exceção nos logs
?? Verificar fila no RabbitMQ Management

### **SignalR não envia?**
?? Nenhum cliente conectado
?? Verificar autenticação JWT
?? Verificar CORS (deve permitir credentials)

---

## ? CHECKLIST DE SUCESSO

- [ ] Breakpoint 1 ? 4 executam sequencialmente
- [ ] RabbitMQ Management mostra mensagem na fila
- [ ] Breakpoint 5 ? 7 executam após alguns segundos
- [ ] Log mostra: "?? SignalR notification sent"
- [ ] Frontend recebe evento (se conectado)

---

**?? PRONTO PARA DEBUGGAR!**

Siga os breakpoints na ordem e você verá o fluxo completo:
**Controller ? Repository ? Handler ? RabbitMQ ? Consumer ? SignalR**
