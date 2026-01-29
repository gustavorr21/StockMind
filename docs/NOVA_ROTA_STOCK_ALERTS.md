# ?? NOVA ROTA - Stock Alerts com Filtros e Paginação

## ? ROTA CRIADA

### **GET /api/stock-alerts**
Busca alertas de estoque com **filtros avançados** e **paginação**.

---

## ?? **ENDPOINT**

```
GET https://localhost:7000/api/stock-alerts?status=Active&page=1&pageSize=10
```

---

## ?? **AUTENTICAÇÃO**

**Requer:** Bearer Token (JWT)

**Roles permitidas:** Admin, Manager, Operator, Viewer

---

## ?? **QUERY PARAMETERS (Todos opcionais)**

| Parâmetro | Tipo | Descrição | Exemplo |
|-----------|------|-----------|---------|
| `status` | `AlertStatus` | Filtrar por status | `Active`, `Acknowledged`, `Resolved` |
| `productId` | `Guid` | Filtrar por produto específico | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| `warehouseId` | `Guid` | Filtrar por depósito | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| `startDate` | `DateTime` | Data inicial (alertas criados após) | `2024-01-01` |
| `endDate` | `DateTime` | Data final (alertas criados antes) | `2024-12-31` |
| `page` | `int` | Número da página (default: 1) | `1` |
| `pageSize` | `int` | Itens por página (default: 10) | `10` |

---

## ?? **RESPOSTA**

### **Status 200 - Success**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "productId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "productName": "Produto ABC",
      "productSku": "PRD-001",
      "warehouseId": "2c8e6b3a-5c95-4d4e-a1b2-8f7e9d3c1a4b",
      "warehouseName": "Depósito Principal",
      "currentQuantity": 45,
      "minimumQuantity": 50,
      "status": "Active",
      "firstDetectedAt": "2024-01-15T10:30:00Z",
      "lastNotifiedAt": "2024-01-15T14:20:00Z",
      "resolvedAt": null,
      "acknowledgedAt": null,
      "notes": null
    },
    {
      "id": "8b7d3e2a-9c5f-4d1e-a3b4-6f8e7c9d2a1b",
      "productId": "4d7e6c9a-8b5f-4d2e-a1b3-7f9e8d3c2a4b",
      "productName": "Produto XYZ",
      "productSku": "PRD-002",
      "warehouseId": "2c8e6b3a-5c95-4d4e-a1b2-8f7e9d3c1a4b",
      "warehouseName": "Depósito Principal",
      "currentQuantity": 15,
      "minimumQuantity": 30,
      "status": "Acknowledged",
      "firstDetectedAt": "2024-01-14T08:15:00Z",
      "lastNotifiedAt": "2024-01-14T08:15:00Z",
      "resolvedAt": null,
      "acknowledgedAt": "2024-01-14T10:30:00Z",
      "notes": "Pedido de compra #1234 criado"
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3,
  "hasPrevious": false,
  "hasNext": true
}
```

### **Status 400 - Bad Request**

```json
{
  "error": "Error getting stock alerts"
}
```

### **Status 401 - Unauthorized**

```json
{
  "error": "Unauthorized"
}
```

---

## ?? **EXEMPLOS DE USO**

### **1. Todos os alertas ativos (primeira página)**
```
GET /api/stock-alerts?status=Active&page=1&pageSize=10
```

### **2. Alertas de um produto específico**
```
GET /api/stock-alerts?productId=3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### **3. Alertas reconhecidos de um depósito**
```
GET /api/stock-alerts?status=Acknowledged&warehouseId=2c8e6b3a-5c95-4d4e-a1b2-8f7e9d3c1a4b
```

### **4. Alertas criados em um período**
```
GET /api/stock-alerts?startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=20
```

### **5. Sem filtros (todos os alertas)**
```
GET /api/stock-alerts?page=1&pageSize=10
```

---

## ?? **STATUS DISPONÍVEIS**

| Status | Descrição |
|--------|-----------|
| `Active` | Alerta ativo (não reconhecido) |
| `Acknowledged` | Alerta reconhecido (em tratamento) |
| `Resolved` | Alerta resolvido (estoque normalizado) |

---

## ?? **TESTE NO SWAGGER**

1. Abrir Swagger: `https://localhost:7000/swagger`
2. Fazer login: `POST /api/auth/login`
3. Copiar o `accessToken`
4. Clicar em **Authorize** e colar o token
5. Expandir `GET /api/stock-alerts`
6. Preencher os filtros desejados
7. Clicar em **Execute**

---

## ?? **DIFERENÇA ENTRE AS ROTAS**

### **GET /api/stock-alerts** ? NOVA!
- ? Filtros avançados
- ? Paginação
- ? Busca por produto, depósito, status, período
- ? Retorna `PagedResult<StockAlertDto>`

### **GET /api/stock-alerts/active** (Antiga)
- ? Sem filtros
- ? Sem paginação
- ? Retorna apenas alertas ativos
- ? Retorna `List<StockAlertDto>`

**Recomendação:** Use a **nova rota** (`GET /api/stock-alerts`) para mais flexibilidade!

---

## ?? **ARQUIVOS CRIADOS**

1. ? `GetStockAlertsQuery.cs` - Query com filtros
2. ? `GetStockAlertsQueryHandler.cs` - Handler com paginação
3. ? `IStockAlertRepository.GetFilteredAlertsAsync()` - Interface
4. ? `StockAlertRepository.GetFilteredAlertsAsync()` - Implementação
5. ? `StockAlertsController.GetAlerts()` - Endpoint

---

## ? **BUILD BEM-SUCEDIDO**

O código foi testado e compilado com sucesso! ??

---

## ?? **PRONTO PARA USAR!**

A rota está 100% funcional e pronta para testar via Swagger ou frontend!
