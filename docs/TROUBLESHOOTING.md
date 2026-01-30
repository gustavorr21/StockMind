# StockMind - Troubleshooting Guide

## Problema: Aplicação falha ao iniciar com código -1 (0xffffffff)

### Sintomas
```
[INF] Database seeded successfully
[INF] Starting StockMind API
C:\...\StockMind.API.exe exited with code -1 (0xffffffff)
```

### Causa
Problema com certificado HTTPS de desenvolvimento não confiável ou IOExceptions no `System.Net.Security.dll`.

### Solução Implementada

1. **Movido o seed do banco para DEPOIS da configuração do pipeline**
   - O seed agora ocorre após `app.MapControllers()`
   - Isso garante que todos os serviços estejam configurados

2. **Adicionado configuração Kestrel para desenvolvimento**
```csharp
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.AllowAnyClientCertificate();
        });
    });
}
```

3. **Melhorado tratamento de exceções**
   - `await app.RunAsync()` ao invés de `app.Run()`
   - `await Log.CloseAndFlushAsync()` para garantir flush dos logs
   - Re-throw de exceções no seed para debug

### Como Verificar o Certificado de Desenvolvimento

#### PowerShell (Windows)
```powershell
# Verificar se o certificado de desenvolvimento existe
dotnet dev-certs https --check

# Se não existir, criar
dotnet dev-certs https --trust
```

#### Verificar Portas
```powershell
# Ver se alguma porta está em uso
netstat -ano | findstr :7000
netstat -ano | findstr :5000
```

### Outros Problemas Comuns

#### 1. Porta já em uso
**Sintoma:** `System.IO.IOException: Failed to bind to address`

**Solução:**
```powershell
# Matar processo na porta
$process = Get-NetTCPConnection -LocalPort 7000 -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Id $process.OwningProcess -Force
}
```

#### 2. SQL Server não está rodando
**Sintoma:** Timeout ao conectar no banco

**Solução:**
```powershell
# Verificar serviço SQL Server
Get-Service -Name 'MSSQL*'

# Iniciar se necessário
Start-Service -Name 'MSSQL$SQLEXPRESS'
```

#### 3. Connection String incorreta
**Sintoma:** `Login failed for user`

**Solução:** Verificar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StockMindDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### 4. Migrations não aplicadas
**Sintoma:** `Invalid object name 'Users'`

**Solução:**
```bash
cd src/StockMind.Infrastructure
dotnet ef database update --startup-project ../StockMind.API
```

### Debug Avançado

#### Habilitar logs detalhados do Kestrel
Em `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Debug",
      "Microsoft.AspNetCore.Server.Kestrel": "Debug"
    }
  }
}
```

#### Ver exceções detalhadas
Execute com:
```bash
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/StockMind.API
```

#### Desabilitar HTTPS temporariamente
Em `appsettings.Development.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

E remover/comentar `app.UseHttpsRedirection()` no `Program.cs`.

### Logs de Debug

Verificar arquivo de log:
```
logs/stockmind-YYYYMMDD.txt
```

### Verificação de Saúde da Aplicação

1. ? Build bem-sucedido
2. ? Banco de dados criado
3. ? Migrations aplicadas
4. ? Seed executado
5. ? **Servidor HTTP iniciado** ? Problema aqui

### Contato

Se o problema persistir após estas soluções:
1. Verifique os logs em `logs/stockmind-*.txt`
2. Execute `dotnet run --verbosity diagnostic`
3. Abra uma issue no GitHub com os logs
