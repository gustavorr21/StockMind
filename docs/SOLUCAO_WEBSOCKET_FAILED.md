# ?? SOLUÇÃO - WebSocket SignalR Connection Failed

## ? **ERRO ORIGINAL**

```
WebSocket connection to 'wss://localhost:49469/hubs/stock-alerts?id=...' failed
```

---

## ? **CORREÇÕES APLICADAS**

### **1?? CORS Configurado Corretamente**

**Arquivo:** `src/StockMind.API/Program.cs`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Em desenvolvimento, permite qualquer origem (para testar HTML local)
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Required for SignalR
        }
        else
        {
            // Em produção, especifica origins permitidas
            policy.WithOrigins("http://localhost:3000", "https://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});
```

**O que mudou:**
- ? `SetIsOriginAllowed(_ => true)` - Permite **qualquer origem** em desenvolvimento
- ? Funciona com HTML aberto diretamente (`file://`)
- ? Funciona com `localhost:49469`
- ? `AllowCredentials()` - Necessário para SignalR

---

### **2?? JWT via Query String (WebSocket)**

**Arquivo:** `src/StockMind.API/Program.cs`

```csharp
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // ... configurações existentes ...
    };
    
    // Configure JWT for SignalR (WebSocket)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            // If the request is for SignalR hub
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});
```

**O que mudou:**
- ? SignalR passa token via **query string** (`?access_token=...`)
- ? JWT Bearer agora lê o token da query string para WebSocket
- ? Funciona para todas as rotas `/hubs/*`

---

### **3?? URL Corrigida no Cliente HTML**

**Arquivo:** `test-signalr-client.html`

```html
<input type="text" id="apiUrl" value="https://localhost:49469" placeholder="https://localhost:49469">
```

**O que mudou:**
- ? URL padrão alterada de `7000` para `49469`
- ? Corresponde à porta configurada em `launchSettings.json`

---

## ?? **COMO TESTAR AGORA**

### **PASSO 1: Reiniciar a API**

```sh
# Parar a API se estiver rodando (Ctrl+C)
# Reiniciar
cd src/StockMind.API
dotnet run
```

### **PASSO 2: Verificar porta**

Confirme que a API está rodando em:
```
https://localhost:49469
```

### **PASSO 3: Obter JWT Token**

1. Abrir Swagger: `https://localhost:49469/swagger`
2. `POST /api/auth/login`
```json
{
  "email": "admin@stockmind.com",
  "password": "Admin@123"
}
```
3. Copiar `accessToken`

### **PASSO 4: Conectar SignalR**

1. Abrir `test-signalr-client.html` no navegador
2. Colar token no campo
3. Clicar em **"Conectar SignalR"**

**Resultado esperado:**
```
[10:30:15] ? Conectado com sucesso! Connection ID: abc123
```

### **PASSO 5: Testar Alerta**

1. No Swagger, criar saída de estoque
2. Ver alerta aparecer em tempo real no HTML

---

## ?? **TROUBLESHOOTING**

### **Erro: CORS Origin Blocked**

**Sintoma:**
```
Access to XMLHttpRequest has been blocked by CORS policy
```

**Solução:**
? Já corrigido com `SetIsOriginAllowed(_ => true)` em desenvolvimento

---

### **Erro: WebSocket Connection Failed**

**Sintomas:**
```
WebSocket connection to 'wss://localhost:49469/hubs/stock-alerts' failed
```

**Possíveis causas:**

#### **1. Certificado SSL inválido**

**Solução:**
No navegador, aceite o certificado:
1. Navegue para `https://localhost:49469`
2. Clique em "Avançado" ? "Continuar para localhost (não seguro)"
3. Volte e teste o SignalR

#### **2. API não está rodando**

**Verificar:**
```sh
# Ver se a API está rodando
curl https://localhost:49469/swagger

# Ou abrir no navegador
https://localhost:49469/swagger
```

#### **3. Porta incorreta**

**Verificar:**
```json
// src/StockMind.API/Properties/launchSettings.json
"applicationUrl": "https://localhost:49469;http://localhost:49470"
```

Se for diferente, atualize:
- `test-signalr-client.html` ? Input default
- Suas requisições

---

### **Erro: 401 Unauthorized**

**Sintoma:**
```
Failed to start the connection: Error: Unauthorized
```

**Causa:**
Token JWT expirado ou inválido

**Solução:**
1. Obter novo token via `/api/auth/login`
2. Colar no cliente HTML
3. Reconectar

---

### **Erro: Hub not found**

**Sintoma:**
```
Error: Failed to complete negotiation with the server: Error: Not Found
```

**Verificar:**
```csharp
// Program.cs deve ter:
app.MapHub<StockMind.API.Hubs.StockAlertHub>("/hubs/stock-alerts");
```

**URL deve ser:**
```
https://localhost:49469/hubs/stock-alerts
```

---

## ?? **LOGS ESPERADOS**

### **Console da API:**
```
[INF] Starting StockMind API
[INF] Now listening on: https://localhost:49469
[INF] Application started. Press Ctrl+C to shut down.
```

### **Navegador (DevTools Console):**
```
[HubConnection] Info: Normalizing '_blank' to ''.
[HubConnection] Info: WebSocket connected to wss://localhost:49469/hubs/stock-alerts?id=...
[10:30:15] ? Conectado com sucesso! Connection ID: abc123
```

### **Cliente HTML:**
```
[10:30:15] ? Página carregada. Pronto para conectar!
[10:30:20] Iniciando conexão com SignalR...
[10:30:21] ?? Conexão estabelecida com sucesso!
[10:30:21] ? Conectado com sucesso! Connection ID: abc123
```

---

## ? **CHECKLIST DE VALIDAÇÃO**

- [ ] API rodando em `https://localhost:49469`
- [ ] Swagger acessível: `https://localhost:49469/swagger`
- [ ] Token JWT obtido via `/api/auth/login`
- [ ] CORS configurado com `SetIsOriginAllowed(_ => true)`
- [ ] JWT configurado para ler `access_token` da query string
- [ ] Hub registrado: `app.MapHub<StockAlertHub>("/hubs/stock-alerts")`
- [ ] Cliente HTML com URL correta: `https://localhost:49469`
- [ ] Certificado SSL aceito no navegador
- [ ] Build bem-sucedido

---

## ?? **FLUXO CORRETO**

```
[Cliente HTML]
    ?
[Pega token do input]
    ?
[SignalR passa token via ?access_token=...]
    ?
[JWT Bearer lê da query string]
    ?
[Valida token]
    ?
[WebSocket conecta com sucesso]
    ?
[Hub.OnConnectedAsync() dispara]
    ?
[Envia "Connected" ao cliente]
    ?
[Cliente recebe connectionId]
    ?
[? CONECTADO!]
```

---

## ?? **PRONTO!**

Todas as correções foram aplicadas. Reinicie a API e teste novamente!

**Se o erro persistir:**
1. Verifique os logs da API no console
2. Abra DevTools do navegador (F12) ? Console
3. Veja mensagens de erro detalhadas
4. Compare com os logs esperados acima
