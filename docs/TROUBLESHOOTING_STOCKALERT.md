# ?? TROUBLESHOOTING - StockAlert não está sendo criado

## ? PROBLEMA RELATADO

Quando o estoque é baixado (saída), os registros **NÃO estão sendo criados** na tabela `StockAlerts`.

---

## ? VERIFICAÇÕES JÁ FEITAS

1. ? `DbSet<StockAlert>` está definido no `ApplicationDbContext`
2. ? `StockAlertConfiguration` está correta
3. ? `CheckAndCreateLowStockAlert()` está sendo chamado no `StockExitCommandHandler`
4. ? Lógica de comparação: `currentQuantity < product.MinimumStock`

---

## ?? DIAGNÓSTICO - POSSÍVEIS CAUSAS

### **1?? Produto NÃO tem `MinimumStock` definido**

**Sintoma:**
```csharp
product.MinimumStock = 0 (padrão)
currentQuantity = 45
// Comparação: 45 < 0? NÃO! ?
```

**Verificar:**
```sql
SELECT Id, Name, Sku, MinimumStock, MaximumStock 
FROM Products
```

**Se `MinimumStock = 0` ou `NULL`:**
- O alerta **NUNCA** será criado
- Você precisa configurar o estoque mínimo do produto

---

### **2?? Exceção está sendo capturada silenciosamente**

**Código atual:**
```csharp
catch (Exception ex)
{
    // Não propaga exceção para não quebrar o fluxo principal
    _logger.LogError(ex, "? Error checking/creating low stock alert...");
}
```

**Verificar logs:**
- Procure por `"? Error checking/creating low stock alert"`
- Se houver exceção, o alerta não é criado MAS a saída de estoque continua

---

### **3?? SaveChanges não está commitando**

**Código atual:**
```csharp
await _stockAlertRepository.AddAsync(alert, cancellationToken);
// ...
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

**Se houver exceção DEPOIS de criar o alerta mas ANTES do SaveChanges:**
- Alerta é criado em memória
- SaveChanges não é chamado
- **Nada é salvo no banco**

---

## ?? TESTES PARA DIAGNOSTICAR

### **TESTE 1: Verificar MinimumStock do Produto**

```http
GET https://localhost:49469/api/products/{productId}
```

**Resposta esperada:**
```json
{
  "id": "...",
  "name": "Produto Teste",
  "minimumStock": 50,  // ?? DEVE SER > 0!
  "maximumStock": 100
}
```

**Se `minimumStock = 0`:**
```http
PUT https://localhost:49469/api/products/{productId}
{
  "minimumStock": 50,
  "maximumStock": 100
}
```

---

### **TESTE 2: Criar Saída e Verificar Logs**

**1. Criar entrada:**
```http
POST https://localhost:49469/api/stock/entry
{
  "productId": "...",
  "warehouseId": "...",
  "quantity": 100,
  "origin": "Purchase"
}
```

**2. Criar saída:**
```http
POST https://localhost:49469/api/stock/exit
{
  "productId": "...",
  "warehouseId": "...",
  "quantity": 55,
  "origin": "Sale"
}
```

**3. Verificar logs da API:**

**LOGS ESPERADOS (SE FUNCIONAR):**
```
[INF] ?? Stock updated: ProductId=..., NewQuantity=45
[INF] ?? CHECKING ALERT: Product=Produto, Current=45, Minimum=50, ProductId=...
[WRN] ?? LOW STOCK DETECTED for product Produto... Current: 45, Minimum: 50
[INF] ?? Creating NEW alert for product ...
[INF] ?? Alert created in memory: AlertId=..., ProductId=...
[INF] ? Alert added to repository (pending SaveChanges): AlertId=...
[INF] ?? Saving changes to database...
[INF] ? Changes saved successfully
[INF] ?? LowStockDetectedEvent published for product ...
```

**LOGS SE NÃO FUNCIONAR (MinimumStock = 0):**
```
[INF] ?? Stock updated: ProductId=..., NewQuantity=45
[INF] ?? CHECKING ALERT: Product=Produto, Current=45, Minimum=0, ProductId=...
[INF] ? Stock level OK for product Produto... Current: 45, Minimum: 0
// ? Para aqui! Não cria alerta!
```

---

### **TESTE 3: Verificar Banco de Dados**

```sql
-- Verifica se tem alertas
SELECT * FROM StockAlerts
ORDER BY CreatedAt DESC

-- Verifica estoque atual
SELECT 
    s.Id,
    p.Name AS ProductName,
    p.MinimumStock,
    s.CurrentQuantity,
    w.Name AS WarehouseName
FROM Stocks s
INNER JOIN Products p ON s.ProductId = p.Id
INNER JOIN Warehouses w ON s.WarehouseId = w.Id
WHERE s.CurrentQuantity < p.MinimumStock
```

**Resultado esperado:**
- Se tem produtos com `CurrentQuantity < MinimumStock`
- **MAS NÃO tem alertas na tabela `StockAlerts`**
- ? PROBLEMA CONFIRMADO!

---

## ?? SOLUÇÕES

### **SOLUÇÃO 1: Configurar MinimumStock do Produto**

**Via API:**
```http
PUT https://localhost:49469/api/products/{productId}
{
  "name": "Produto Teste",
  "description": "...",
  "sku": "...",
  "minimumStock": 50,  // ?? IMPORTANTE!
  "maximumStock": 100,
  "categoryId": "...",
  ...
}
```

**Via SQL (temporário):**
```sql
UPDATE Products
SET MinimumStock = 50,
    MaximumStock = 100
WHERE Id = '<product-guid>'
```

---

### **SOLUÇÃO 2: Verificar Exceções nos Logs**

**Se houver erro:**
```
[ERR] ? Error checking/creating low stock alert for product ...
```

**Verificar:**
1. Foreign Keys (ProductId, WarehouseId existem?)
2. Validações do `StockAlert.Create()`
3. Permissões no banco de dados

---

### **SOLUÇÃO 3: Adicionar Logs Detalhados (JÁ FEITO)**

Os logs foram adicionados com emojis para facilitar rastreamento:

- ?? `CHECKING ALERT` - Início da verificação
- ?? `LOW STOCK DETECTED` - Alerta detectado
- ?? `Creating NEW alert` - Criando novo alerta
- ?? `Alert created in memory` - Alerta em memória
- ? `Alert added to repository` - Adicionado ao repositório
- ?? `Saving changes` - Salvando no banco
- ? `Changes saved` - Salvo com sucesso

---

## ?? CHECKLIST DE DIAGNÓSTICO

Execute na ordem:

- [ ] **1. Verificar MinimumStock do produto**
  ```sql
  SELECT Id, Name, MinimumStock FROM Products WHERE Id = '<guid>'
  ```
  - Se `MinimumStock = 0` ? **ESSA É A CAUSA!**

- [ ] **2. Criar saída de estoque e verificar logs**
  - Procure por `?? CHECKING ALERT`
  - Veja os valores: `Current` e `Minimum`
  - Se `Current >= Minimum` ? Alerta não será criado

- [ ] **3. Verificar se há exceções**
  - Procure por `? Error checking/creating low stock alert`
  - Se houver, veja o stack trace

- [ ] **4. Verificar banco de dados**
  ```sql
  SELECT COUNT(*) FROM StockAlerts
  ```
  - Se = 0 e deveria ter alertas ? Problema confirmado

- [ ] **5. Verificar UnitOfWork.SaveChanges**
  - Procure por `?? Saving changes to database`
  - Procure por `? Changes saved successfully`
  - Se não aparecer ? SaveChanges não está sendo chamado

---

## ?? CAUSA MAIS PROVÁVEL

### **90% de chance:**

> **O produto NÃO tem `MinimumStock` configurado (valor = 0)**

**Por quê?**
```csharp
// Código: StockExitCommandHandler.cs linha 143
if (currentQuantity >= product.MinimumStock)
{
    _logger.LogInformation("? Stock level OK...");
    return; // ? Para aqui!
}

// Se MinimumStock = 0 e currentQuantity = 45
// Comparação: 45 >= 0? SIM!
// Resultado: Retorna SEM criar alerta
```

---

## ? SOLUÇÃO RÁPIDA

1. **Configurar produto com estoque mínimo:**

```http
PUT https://localhost:49469/api/products/{productId}
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Produto Teste Alerta",
  "description": "Teste",
  "sku": "TST-001",
  "price": {
    "amount": 100.00,
    "currency": "BRL"
  },
  "costPrice": {
    "amount": 50.00,
    "currency": "BRL"
  },
  "categoryId": "{category-guid}",
  "minimumStock": 50,  // ?? CHAVE!
  "maximumStock": 100
}
```

2. **Criar entrada de estoque (100 unidades)**

3. **Criar saída de estoque (55 unidades)**

4. **Verificar logs:**
```
?? CHECKING ALERT: ... Current=45, Minimum=50
?? LOW STOCK DETECTED...
?? Creating NEW alert...
? Alert added to repository...
?? Saving changes...
? Changes saved successfully
```

5. **Consultar alertas:**
```http
GET https://localhost:49469/api/stock-alerts?status=Active
```

**Deve retornar:**
```json
{
  "items": [
    {
      "id": "...",
      "productName": "Produto Teste Alerta",
      "currentQuantity": 45,
      "minimumQuantity": 50,
      "status": "Active"
    }
  ]
}
```

---

## ?? PRÓXIMOS PASSOS

1. Reinicie a API para aplicar os novos logs
2. Execute o TESTE 2 acima
3. Copie os logs da API e envie para análise
4. Verifique o banco: `SELECT * FROM StockAlerts`
5. Verifique o produto: `SELECT MinimumStock FROM Products WHERE Id = '...'`

---

**Com os logs detalhados, conseguiremos identificar exatamente onde está o problema!** ??
