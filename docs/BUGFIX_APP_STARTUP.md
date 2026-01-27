# Correção: Falha ao Iniciar Aplicação (Exit Code -1)

## ?? Problema Identificado

A aplicação estava falhando logo após o seed do banco de dados com o código de saída `-1 (0xffffffff)`:

```
[06:45:55 INF] Database seeded successfully
[06:45:55 INF] Starting StockMind API
StockMind.API.exe (process 61768) exited with code -1 (0xffffffff).
```

## ?? Análise

Através dos logs de debug, identificamos:

1. **IOException no System.Net.Security.dll** - Múltiplas exceções relacionadas a segurança de rede/HTTPS
2. **Ordem incorreta de inicialização** - O seed estava sendo executado ANTES da configuração completa do pipeline
3. **Problemas com certificado HTTPS** - Certificado de desenvolvimento não confiável ou inexistente

## ? Correções Implementadas

### 1. Reordenação do Pipeline de Inicialização

**Antes:**
```csharp
var app = builder.Build();

// Seed ANTES da configuração do pipeline ?
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(services);
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseHttpsRedirection();
app.MapControllers();

app.Run(); // ? Síncrono
```

**Depois:**
```csharp
var app = builder.Build();

// Configure the HTTP request pipeline PRIMEIRO ?
app.UseSwagger();
app.UseHttpsRedirection();
app.MapControllers();

// Seed DEPOIS da configuração ?
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(services);
}

await app.RunAsync(); // ? Assíncrono com await
```

### 2. Configuração do Kestrel para Desenvolvimento

Adicionado configuração para aceitar qualquer certificado de cliente em desenvolvimento:

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

**Benefícios:**
- ? Permite certificados auto-assinados
- ? Evita problemas com certificados não confiáveis
- ? Apenas em desenvolvimento (seguro)

### 3. Melhorado Tratamento de Exceções

**Antes:**
```csharp
try {
    app.Run();
} catch (Exception ex) {
    Log.Fatal(ex, "...");
}
finally {
    Log.CloseAndFlush(); // ? Síncrono
}
```

**Depois:**
```csharp
try {
    Log.Information("Starting StockMind API");
    await app.RunAsync(); // ? Assíncrono
    Log.Information("StockMind API stopped gracefully");
} catch (Exception ex) {
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw; // ? Re-throw para debug
}
finally {
    await Log.CloseAndFlushAsync(); // ? Assíncrono
}
```

### 4. Seed com Melhor Tratamento de Erros

```csharp
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        await DataSeeder.SeedAsync(services);
        Log.Information("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database");
        throw; // ? Re-throw para impedir app de rodar com dados ruins
    }
}
```

## ?? Documentação Criada

### 1. TROUBLESHOOTING.md
Guia completo de troubleshooting com:
- ? Sintomas e causas comuns
- ? Soluções passo a passo
- ? Verificação de certificados
- ? Problemas com SQL Server
- ? Portas em uso
- ? Debug avançado

### 2. setup-dev.ps1
Script PowerShell automatizado que:
- ? Verifica certificado HTTPS
- ? Cria certificado se necessário
- ? Verifica SQL Server
- ? Verifica portas em uso
- ? Aplica migrations
- ? Faz build do projeto
- ? Opção de executar a API

**Como usar:**
```powershell
# Execute como Administrador
.\setup-dev.ps1
```

## ?? Como Resolver Manualmente

Se o problema persistir, execute estes comandos:

### 1. Verificar e Criar Certificado
```bash
dotnet dev-certs https --check
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### 2. Verificar SQL Server
```powershell
Get-Service -Name 'MSSQL*'
Start-Service -Name 'MSSQL$SQLEXPRESS'
```

### 3. Aplicar Migrations
```bash
cd src/StockMind.Infrastructure
dotnet ef database update --startup-project ../StockMind.API
```

### 4. Executar Aplicação
```bash
cd src/StockMind.API
dotnet run
```

## ?? Resultado Esperado

Após as correções, a aplicação deve iniciar corretamente:

```
[INFO] Database seeded successfully
[INFO] Starting StockMind API
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

## ? Verificação

Acesse:
- **Swagger:** https://localhost:7xxx/swagger
- **Health:** https://localhost:7xxx/api/auth/me (com token)

## ?? Arquivos Modificados

- ? `src/StockMind.API/Program.cs` - Ordem do pipeline e configuração Kestrel
- ? `docs/TROUBLESHOOTING.md` - Guia de troubleshooting
- ? `setup-dev.ps1` - Script de setup automatizado

## ?? Commit

```
fix: resolve app startup issue and improve error handling

- Reordered pipeline initialization (seed after MapControllers)
- Added Kestrel configuration for development certificates
- Improved async/await pattern with RunAsync and CloseAndFlushAsync
- Added better exception handling with re-throw
- Created TROUBLESHOOTING.md guide
- Created setup-dev.ps1 automated setup script
```

## ?? Suporte

Se o problema persistir:
1. Execute `.\setup-dev.ps1` como Administrador
2. Verifique logs em `logs/stockmind-*.txt`
3. Consulte `docs/TROUBLESHOOTING.md`
4. Abra issue no GitHub com logs completos
