# StockMind - Certificate Setup Script
# Este script configura o certificado de desenvolvimento HTTPS

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  StockMind - Setup de Certificado HTTPS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se está rodando como administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "??  AVISO: Execute este script como Administrador para melhores resultados" -ForegroundColor Yellow
    Write-Host ""
}

# 1. Verificar certificado de desenvolvimento
Write-Host "1. Verificando certificado de desenvolvimento..." -ForegroundColor Green
try {
    $certCheck = dotnet dev-certs https --check 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Certificado de desenvolvimento encontrado" -ForegroundColor Green
    } else {
        Write-Host "? Certificado de desenvolvimento não encontrado" -ForegroundColor Red
        
        Write-Host "`n2. Criando e confiando no certificado..." -ForegroundColor Green
        dotnet dev-certs https --clean
        dotnet dev-certs https --trust
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Certificado criado e confiável" -ForegroundColor Green
        } else {
            Write-Host "? Falha ao criar certificado" -ForegroundColor Red
            Write-Host "   Tente executar como Administrador" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "? Erro ao verificar certificado: $_" -ForegroundColor Red
}

# 2. Verificar se SQL Server está rodando
Write-Host "`n3. Verificando SQL Server..." -ForegroundColor Green
try {
    $sqlService = Get-Service -Name 'MSSQL*' -ErrorAction SilentlyContinue | Where-Object { $_.Status -eq 'Running' }
    if ($sqlService) {
        Write-Host "? SQL Server está rodando: $($sqlService.Name)" -ForegroundColor Green
    } else {
        Write-Host "? SQL Server não está rodando" -ForegroundColor Red
        $allSqlServices = Get-Service -Name 'MSSQL*' -ErrorAction SilentlyContinue
        if ($allSqlServices) {
            Write-Host "`nServiços SQL Server encontrados:" -ForegroundColor Yellow
            foreach ($service in $allSqlServices) {
                Write-Host "  - $($service.Name): $($service.Status)" -ForegroundColor Yellow
            }
            
            Write-Host "`nTentando iniciar SQL Server..." -ForegroundColor Green
            foreach ($service in $allSqlServices) {
                if ($service.Status -eq 'Stopped') {
                    try {
                        Start-Service $service.Name
                        Write-Host "? Serviço $($service.Name) iniciado" -ForegroundColor Green
                    } catch {
                        Write-Host "? Falha ao iniciar $($service.Name): $_" -ForegroundColor Red
                    }
                }
            }
        } else {
            Write-Host "? Nenhum serviço SQL Server encontrado" -ForegroundColor Red
            Write-Host "   Instale o SQL Server ou SQL Server Express" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "? Erro ao verificar SQL Server: $_" -ForegroundColor Red
}

# 3. Verificar portas
Write-Host "`n4. Verificando portas..." -ForegroundColor Green
$ports = @(5000, 5001, 7000, 7001)
foreach ($port in $ports) {
    try {
        $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
        if ($connection) {
            Write-Host "??  Porta $port está em uso pelo processo $($connection.OwningProcess)" -ForegroundColor Yellow
            $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "   Processo: $($process.ProcessName)" -ForegroundColor Yellow
            }
        } else {
            Write-Host "? Porta $port está disponível" -ForegroundColor Green
        }
    } catch {
        Write-Host "? Porta $port está disponível" -ForegroundColor Green
    }
}

# 4. Verificar banco de dados
Write-Host "`n5. Verificando banco de dados..." -ForegroundColor Green
Push-Location
try {
    Set-Location "src\StockMind.Infrastructure"
    $dbCheck = dotnet ef database update --startup-project ..\StockMind.API --dry-run 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Banco de dados configurado corretamente" -ForegroundColor Green
    } else {
        Write-Host "??  Banco de dados precisa de atualização" -ForegroundColor Yellow
        Write-Host "`nAplicando migrations..." -ForegroundColor Green
        dotnet ef database update --startup-project ..\StockMind.API
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Migrations aplicadas com sucesso" -ForegroundColor Green
        } else {
            Write-Host "? Falha ao aplicar migrations" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "? Erro ao verificar banco de dados: $_" -ForegroundColor Red
} finally {
    Pop-Location
}

# 5. Build do projeto
Write-Host "`n6. Verificando build do projeto..." -ForegroundColor Green
try {
    Push-Location
    Set-Location "src\StockMind.API"
    dotnet build --no-restore > $null 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Build bem-sucedido" -ForegroundColor Green
    } else {
        Write-Host "? Build falhou" -ForegroundColor Red
        Write-Host "   Execute: dotnet build" -ForegroundColor Yellow
    }
    Pop-Location
} catch {
    Write-Host "? Erro ao fazer build: $_" -ForegroundColor Red
}

# Resumo
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Resumo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para iniciar a aplicação:" -ForegroundColor Green
Write-Host "  cd src\StockMind.API" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Credenciais Admin:" -ForegroundColor Green
Write-Host "  Email: admin@stockmind.com" -ForegroundColor White
Write-Host "  Senha: Admin@123" -ForegroundColor White
Write-Host ""
Write-Host "Swagger URL:" -ForegroundColor Green
Write-Host "  https://localhost:7xxx/swagger" -ForegroundColor White
Write-Host ""

# Perguntar se quer executar agora
Write-Host "Deseja iniciar a aplicação agora? (S/N): " -ForegroundColor Yellow -NoNewline
$response = Read-Host
if ($response -eq 'S' -or $response -eq 's') {
    Write-Host "`nIniciando StockMind API..." -ForegroundColor Green
    Set-Location "src\StockMind.API"
    dotnet run
}
