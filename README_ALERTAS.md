# ?? GUIA RÁPIDO - TESTAR SISTEMA DE ALERTAS

## ? INÍCIO RÁPIDO (5 MINUTOS)

### 1?? SUBIR DOCKER
```sh
# Na raiz do projeto
docker-compose up -d

# Verificar se estão rodando
docker ps
```

### 2?? EXECUTAR API
```sh
cd src/StockMind.API
dotnet run
```

### 3?? ACESSAR SWAGGER
```
https://localhost:7000/swagger
```

### 4?? FAZER LOGIN
**POST /api/auth/login**
```json
{
  "email": "admin@stockmind.com",
  "password": "Admin@123"
}
```

Copiar `accessToken` ? Clicar em **Authorize** ? Colar token

### 5?? OBTER IDs

**GET /api/products** ? Copiar `id` do produto

**GET /api/warehouse** ? Copiar `id` do depósito

### 6?? CRIAR ENTRADA
**POST /api/stock/entry**
```json
{
  "productId": "cole-aqui-o-guid-do-produto",
  "warehouseId": "cole-aqui-o-guid-do-deposito",
  "quantity": 100,
  "origin": "Purchase"
}
```

### 7?? CRIAR SAÍDA (GERAR ALERTA) ??
**POST /api/stock/exit**
```json
{
  "productId": "mesmo-guid-do-produto",
  "warehouseId": "mesmo-guid-do-deposito",
  "quantity": 55,
  "origin": "Sale",
  "observation": "Teste de alerta"
}
```

**Resultado:** 
- CurrentQuantity = 45
- MinimumStock = 50
- **ALERTA GERADO!** ??

### 8?? VERIFICAR ALERTAS
**GET /api/stock-alerts/active**

```json
[
  {
    "id": "guid-do-alerta",
    "productName": "Nome do Produto",
    "currentQuantity": 45,
    "minimumQuantity": 50,
    "status": "Active"
  }
]
```

### 9?? RECONHECER ALERTA
**POST /api/stock-alerts/{id}/acknowledge**
```json
{
  "notes": "Pedido de compra criado"
}
```

---

## ? PRONTO! SISTEMA FUNCIONANDO!

**Logs esperados:**
```
[INF] Low stock detected for product...
[INF] Low stock event published...
[INF] Event LowStockDetectedEvent would be published (dummy)
```

---

## ?? PROBLEMAS COMUNS

### Docker não sobe?
```sh
# Verificar se porta 1433 está livre
netstat -ano | findstr :1433

# Se ocupada, mudar porta no docker-compose.yml
```

### Connection String
Se usar Docker, atualizar `appsettings.json`:
```json
"DefaultConnection": "Server=localhost,1433;Database=StockMindDb;User Id=sa;Password=StockMind@2024;TrustServerCertificate=True;"
```

### Alerta não criou?
Verificar:
1. CurrentQuantity < MinimumStock? ?
2. Logs mostram "Low stock detected"? ?
3. Tabela StockAlerts tem registro? ?

---

## ?? DOCUMENTAÇÃO COMPLETA

Ver: `docs/GUIA_TESTES_ALERTAS.md`

---

**SISTEMA 100% FUNCIONAL!** ??
