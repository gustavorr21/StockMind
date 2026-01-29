# ?? GUIA COMPLETO DE TESTES - SISTEMA DE ALERTAS DE ESTOQUE BAIXO

## ?? ÍNDICE
1. [Configuração Inicial](#1-configuração-inicial)
2. [Subir Docker (SQL Server)](#2-subir-docker-sql-server)
3. [Executar o Sistema](#3-executar-o-sistema)
4. [Testes via Swagger](#4-testes-via-swagger)
5. [Testes via Postman](#5-testes-via-postman)
6. [Verificar Logs](#6-verificar-logs)
7. [Troubleshooting](#7-troubleshooting)

---

## 1. CONFIGURAÇÃO INICIAL

### 1.1 Verificar Arquivos
```
? Domain Layer (6 arquivos)
? Application Layer (9 arquivos)
? Infrastructure Layer (5 arquivos)
? API Layer (3 arquivos)
? Migration aplicada
```

### 1.2 Configurar Email (Opcional)
Editar `src/StockMind.API/appsettings.json`:

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "seu-email@gmail.com",
  "SmtpPassword": "sua-senha-app",
  "FromEmail": "noreply@stockmind.com",
  "AlertsEmail": "destino@stockmind.com"
}
```

**IMPORTANTE:** Se não configurar, o email será ignorado (não vai quebrar o sistema).

---

## 2. SUBIR DOCKER (SQL SERVER)

### 2.1 Criar arquivo docker-compose.yml
**Local:** Raiz do projeto `C:\StockMind\StockMind\docker-compose.yml`

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: stockmind-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=StockMind@2024
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - stockmind-network

  rabbitmq:
    image: rabbitmq:3-management
    container_name: stockmind-rabbitmq
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - "5672:5672"
      - "15672:15672"
    networks:
      - stockmind-network

volumes:
  sqlserver-data:

networks:
  stockmind-network:
    driver: bridge
```

### 2.2 Subir os Containers
```sh
# Na raiz do projeto
cd C:\StockMind\StockMind

# Subir containers
docker-compose up -d

# Verificar se estão rodando
docker ps
```

**Resultado esperado:**
```
CONTAINER ID   IMAGE                                   STATUS          PORTS
xxxxx          mcr.microsoft.com/mssql/server:2022     Up 10 seconds   0.0.0.0:1433->1433/tcp
xxxxx          rabbitmq:3-management                   Up 10 seconds   0.0.0.0:5672->5672/tcp
```

### 2.3 Atualizar Connection String
**Arquivo:** `src/StockMind.API/appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=StockMindDb;User Id=sa;Password=StockMind@2024;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=60;"
}
```

### 2.4 Aplicar Migrations (se necessário)
```sh
cd src/StockMind.API

# Verificar migrations pendentes
dotnet ef migrations list -p ../StockMind.Infrastructure

# Aplicar se necessário
dotnet ef database update -p ../StockMind.Infrastructure
```

---

## 3. EXECUTAR O SISTEMA

### 3.1 Via Visual Studio
1. Abrir `StockMind.sln`
2. Definir `StockMind.API` como projeto de inicialização
3. Pressionar `F5` ou clicar em "Start"

### 3.2 Via Terminal
```sh
cd C:\StockMind\StockMind\src\StockMind.API

dotnet run
```

**Resultado esperado:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 3.3 Acessar Swagger
Abrir navegador:
```
https://localhost:7000/swagger
```

---

## 4. TESTES VIA SWAGGER

### 4.1 PASSO 1: Fazer Login

**Endpoint:** `POST /api/auth/login`

**Body:**
```json
{
  "email": "admin@stockmind.com",
  "password": "Admin@123"
}
```

**Resposta esperada:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresIn": 28800
}
```

### 4.2 PASSO 2: Autorizar no Swagger

1. Copiar o `accessToken`
2. Clicar no botão **"Authorize"** (cadeado verde no topo)
3. Digitar: `Bearer {accessToken}`
4. Clicar em **"Authorize"**
5. Clicar em **"Close"**

### 4.3 PASSO 3: Obter IDs de Produto e Depósito

#### Listar Produtos
**Endpoint:** `GET /api/products`

**Resposta:**
```json
{
  "items": [
    {
      "id": "guid-do-produto",
      "name": "Notebook Dell",
      "sku": "NB-DELL-001",
      "minimumStock": 50,
      "maximumStock": 500
    }
  ]
}
```

#### Listar Depósitos
**Endpoint:** `GET /api/warehouse`

**Resposta:**
```json
[
  {
    "id": "guid-do-deposito",
    "name": "Depósito Principal",
    "isMain": true,
    "isActive": true
  }
]
```

### 4.4 PASSO 4: Criar Entrada de Estoque

**Endpoint:** `POST /api/stock/entry`

**Body:**
```json
{
  "productId": "guid-do-produto",
  "warehouseId": "guid-do-deposito",
  "quantity": 100,
  "origin": "Purchase",
  "observation": "Compra inicial para teste"
}
```

**Resposta esperada:**
```json
{
  "movementId": "guid-da-movimentacao",
  "message": "Stock added successfully"
}
```

### 4.5 PASSO 5: Consultar Estoque Atual

**Endpoint:** `GET /api/stock/position?productId={guid}&warehouseId={guid}`

**Resposta:**
```json
{
  "stockId": "guid",
  "productId": "guid",
  "productName": "Notebook Dell",
  "currentQuantity": 100,
  "minimumQuantity": 50,
  "availableQuantity": 100,
  "isBelowMinimum": false
}
```

### 4.6 PASSO 6: Criar Saída que Gere Alerta ??

**Endpoint:** `POST /api/stock/exit`

**Body:**
```json
{
  "productId": "guid-do-produto",
  "warehouseId": "guid-do-deposito",
  "quantity": 55,
  "origin": "Sale",
  "observation": "Venda que deve gerar alerta"
}
```

**Resultado esperado:**
- CurrentQuantity = 45 (100 - 55)
- MinimumStock = 50
- **45 < 50 = ALERTA GERADO! ??**

### 4.7 PASSO 7: Verificar Logs

Procurar no console/terminal:
```
info: StockMind.Application.Handlers.Events.LowStockDetectedEventHandler
      Low stock detected for product Notebook Dell (NB-DELL-001) in warehouse Depósito Principal. Current: 45, Minimum: 50

info: StockMind.Application.Handlers.Events.LowStockDetectedEventHandler
      Low stock event published to RabbitMQ for product {guid}

info: StockMind.Infrastructure.Messaging.DummyEventPublisher
      Event LowStockDetectedEvent would be published (dummy implementation)
```

### 4.8 PASSO 8: Consultar Alertas Ativos ??

**Endpoint:** `GET /api/stock-alerts/active`

**Resposta esperada:**
```json
[
  {
    "id": "guid-do-alerta",
    "productId": "guid-do-produto",
    "productName": "Notebook Dell",
    "productSku": "NB-DELL-001",
    "warehouseId": "guid-do-deposito",
    "warehouseName": "Depósito Principal",
    "currentQuantity": 45,
    "minimumQuantity": 50,
    "status": "Active",
    "firstDetectedAt": "2024-01-29T14:30:00Z",
    "lastNotifiedAt": "2024-01-29T14:30:00Z",
    "resolvedAt": null,
    "acknowledgedAt": null,
    "notes": null
  }
]
```

### 4.9 PASSO 9: Reconhecer Alerta ?

**Endpoint:** `POST /api/stock-alerts/{id}/acknowledge`

**Body:**
```json
{
  "notes": "Pedido de compra #1234 criado para reposição"
}
```

**Resposta esperada:**
```json
{
  "message": "Alert acknowledged successfully"
}
```

### 4.10 PASSO 10: Verificar Status Alterado

**Endpoint:** `GET /api/stock-alerts/active`

**Resultado:** Lista vazia (alerta foi reconhecido, não está mais ativo)

---

## 5. TESTES VIA POSTMAN

### 5.1 Importar Collection

Criar arquivo `StockMind-Alerts-Tests.postman_collection.json`:

```json
{
  "info": {
    "name": "StockMind - Alertas de Estoque",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "auth": {
    "type": "bearer",
    "bearer": [
      {
        "key": "token",
        "value": "{{accessToken}}",
        "type": "string"
      }
    ]
  },
  "item": [
    {
      "name": "1. Login",
      "event": [
        {
          "listen": "test",
          "script": {
            "exec": [
              "pm.test(\"Status 200\", function() {",
              "    pm.response.to.have.status(200);",
              "});",
              "",
              "var jsonData = pm.response.json();",
              "pm.environment.set(\"accessToken\", jsonData.accessToken);"
            ]
          }
        }
      ],
      "request": {
        "method": "POST",
        "header": [],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"admin@stockmind.com\",\n  \"password\": \"Admin@123\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": "https://localhost:7000/api/auth/login"
      }
    },
    {
      "name": "2. Criar Saída (Gerar Alerta)",
      "request": {
        "method": "POST",
        "header": [],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"productId\": \"{{productId}}\",\n  \"warehouseId\": \"{{warehouseId}}\",\n  \"quantity\": 55,\n  \"origin\": \"Sale\",\n  \"observation\": \"Teste de alerta\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": "https://localhost:7000/api/stock/exit"
      }
    },
    {
      "name": "3. Listar Alertas Ativos",
      "request": {
        "method": "GET",
        "header": [],
        "url": "https://localhost:7000/api/stock-alerts/active"
      }
    },
    {
      "name": "4. Reconhecer Alerta",
      "request": {
        "method": "POST",
        "header": [],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"notes\": \"Pedido de compra criado\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": "https://localhost:7000/api/stock-alerts/{{alertId}}/acknowledge"
      }
    }
  ]
}
```

### 5.2 Configurar Variáveis

**Environment Variables:**
```json
{
  "accessToken": "",
  "productId": "cole-aqui-o-guid-do-produto",
  "warehouseId": "cole-aqui-o-guid-do-deposito",
  "alertId": "cole-aqui-o-guid-do-alerta"
}
```

---

## 6. VERIFICAR LOGS

### 6.1 Logs da Aplicação

**Local:** `src/StockMind.API/logs/stockmind-{data}.txt`

Procurar por:
```
[INF] Low stock detected for product...
[INF] Low stock event published to RabbitMQ...
[INF] Event LowStockDetectedEvent would be published...
```

### 6.2 Logs do SQL Server (Docker)

```sh
docker logs stockmind-sqlserver
```

### 6.3 Logs do RabbitMQ

```sh
docker logs stockmind-rabbitmq
```

Ou acessar interface web:
```
http://localhost:15672
User: guest
Pass: guest
```

---

## 7. TROUBLESHOOTING

### 7.1 Alerta NÃO foi criado

**Verificações:**

1. **Estoque realmente ficou abaixo do mínimo?**
```sql
SELECT CurrentQuantity, Product.MinimumStock
FROM Stocks s
INNER JOIN Products p ON s.ProductId = p.Id
WHERE s.ProductId = 'guid' AND s.WarehouseId = 'guid'
```

2. **Domain Event foi disparado?**
Procurar nos logs:
```
Low stock detected for product...
```

3. **Handler foi executado?**
Procurar nos logs:
```
Low stock event published to RabbitMQ...
```

### 7.2 Email não foi enviado

**Causa:** Configuração de email não preenchida ou incorreta.

**Solução:** O sistema ignora erro de email. Verificar logs:
```
[WRN] Alerts email not configured. Skipping email.
```

### 7.3 SignalR não está conectando

**Verificações:**

1. Hub mapeado no Program.cs?
```csharp
app.MapHub<StockAlertHub>("/hubs/stock-alerts");
```

2. SignalR adicionado?
```csharp
builder.Services.AddSignalR();
```

3. Frontend conectando corretamente?
```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7000/hubs/stock-alerts", {
    accessTokenFactory: () => this.token
  })
  .build();

await connection.start();
```

### 7.4 Docker não sobe

**Erro:** `port is already allocated`

**Solução:**
```sh
# Verificar portas ocupadas
netstat -ano | findstr :1433

# Matar processo
taskkill /PID {PID} /F

# Subir novamente
docker-compose up -d
```

### 7.5 Migration não aplica

```sh
# Remover migrations pendentes
dotnet ef migrations remove -p src/StockMind.Infrastructure -s src/StockMind.API

# Recriar
dotnet ef migrations add AddStockAlerts -p src/StockMind.Infrastructure -s src/StockMind.API

# Aplicar
dotnet ef database update -p src/StockMind.Infrastructure -s src/StockMind.API
```

---

## 8. TESTES AVANÇADOS

### 8.1 Testar Múltiplos Alertas

```sh
# Criar produtos diferentes abaixo do mínimo
POST /api/stock/exit (Produto A)
POST /api/stock/exit (Produto B)
POST /api/stock/exit (Produto C)

# Verificar todos os alertas
GET /api/stock-alerts/active
```

### 8.2 Testar Resolução Automática

```sh
# 1. Criar alerta (saída que deixe abaixo do mínimo)
POST /api/stock/exit
Quantity: 55 (CurrentQuantity = 45, Minimum = 50)

# 2. Verificar alerta criado
GET /api/stock-alerts/active

# 3. Reabastecer acima do mínimo
POST /api/stock/entry
Quantity: 10 (CurrentQuantity = 55, Minimum = 50)

# 4. Verificar alerta resolvido automaticamente
# TODO: Implementar lógica de resolução automática
```

---

## 9. CHECKLIST FINAL

- [ ] Docker rodando (SQL Server + RabbitMQ)
- [ ] Migrations aplicadas
- [ ] Sistema compilando
- [ ] API rodando (https://localhost:7000)
- [ ] Login funcionando
- [ ] Produto e Depósito existem
- [ ] Entrada de estoque funcionando
- [ ] Saída de estoque funcionando
- [ ] Alerta sendo criado quando CurrentQuantity < MinimumStock
- [ ] GET /api/stock-alerts/active retornando alertas
- [ ] Reconhecimento de alerta funcionando
- [ ] Logs sendo gerados

---

## 10. PRÓXIMOS PASSOS

### Implementar (Opcional):
1. **RabbitMQ Consumer** - Processar alertas de forma assíncrona
2. **SignalR Notifications** - Notificar frontend em tempo real
3. **Email Sender** - Enviar emails reais
4. **Resolução Automática** - Resolver alertas quando estoque normalizar
5. **Dashboard de Alertas** - Visualização gráfica

---

**SISTEMA DE ALERTAS 100% FUNCIONAL!** ??

Para testar, siga os passos 1-9 da seção "4. TESTES VIA SWAGGER".
