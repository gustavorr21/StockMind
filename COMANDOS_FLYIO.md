# ?? Comandos Práticos - Deploy StockMind

Copie e cole estes comandos diretamente no terminal. Substitua os valores entre `<...>` pelos seus valores reais.

## ?? Setup Inicial (Uma Vez)

### 1. Instalar Fly CLI

**Windows (PowerShell como Administrador):**
```powershell
iwr https://fly.io/install.ps1 -useb | iex
```

**Mac:**
```bash
brew install flyctl
```

**Linux:**
```bash
curl -L https://fly.io/install.sh | sh
```

### 2. Login no Fly.io

```bash
flyctl auth login
```

## ??? Criar e Configurar App

### Opção A: Deploy Automático (Mais Fácil)

```bash
# Vai no diretório do projeto
cd C:\StockMind\StockMind

# Deploy tudo de uma vez
flyctl launch --name stockmind --region ams --now
```

### Opção B: Passo a Passo (Mais Controle)

```bash
# 1. Criar app
flyctl apps create stockmind --org personal

# 2. Criar banco PostgreSQL (OPCIONAL)
flyctl postgres create --name stockmind-db --region ams --initial-cluster-size 1

# 3. Anexar banco ao app (se criou no passo 2)
flyctl postgres attach stockmind-db --app stockmind

# 4. Configurar secrets OBRIGATÓRIOS
flyctl secrets set JwtSettings__Secret="StockMind-Super-Secret-Key-For-JWT-Token-Generation-2024-Minimum-32-Characters" --app stockmind

# 5. Deploy!
flyctl deploy --app stockmind
```

## ?? Configurar Secrets

### JWT (OBRIGATÓRIO)

```bash
flyctl secrets set JwtSettings__Secret="StockMind-Super-Secret-Key-For-JWT-Token-Generation-2024-Minimum-32-Characters" --app stockmind
```

### Banco de Dados (Se não usar Fly Postgres)

```bash
# SQL Server
flyctl secrets set ConnectionStrings__DefaultConnection="Server=<seu-server>;Database=StockMindDb;User Id=<usuario>;Password=<senha>;TrustServerCertificate=True;" --app stockmind

# PostgreSQL
flyctl secrets set ConnectionStrings__DefaultConnection="Host=<host>;Port=5432;Database=stockmind;Username=<usuario>;Password=<senha>;SSL Mode=Require;" --app stockmind
```

### Email (OPCIONAL)

```bash
# Gmail
flyctl secrets set Email__SmtpHost="smtp.gmail.com" --app stockmind
flyctl secrets set Email__SmtpPort="587" --app stockmind
flyctl secrets set Email__FromEmail="seu-email@gmail.com" --app stockmind
flyctl secrets set Email__Password="sua-senha-app-google" --app stockmind

# Outlook/Hotmail
flyctl secrets set Email__SmtpHost="smtp-mail.outlook.com" --app stockmind
flyctl secrets set Email__SmtpPort="587" --app stockmind
```

### RabbitMQ (OPCIONAL)

```bash
# CloudAMQP ou outro
flyctl secrets set RabbitMQ__HostName="<seu-host>.cloudamqp.com" --app stockmind
flyctl secrets set RabbitMQ__UserName="<usuario>" --app stockmind
flyctl secrets set RabbitMQ__Password="<senha>" --app stockmind
flyctl secrets set RabbitMQ__Port="5672" --app stockmind
```

### Redis (OPCIONAL)

```bash
# Redis Labs ou outro
flyctl secrets set Redis__Configuration="<host>:<port>,password=<senha>" --app stockmind
```

## ?? Deploy e Atualizações

### Deploy Inicial ou Nova Versão

```bash
cd C:\StockMind\StockMind
flyctl deploy --app stockmind
```

### Deploy com Logs em Tempo Real

```bash
flyctl deploy --app stockmind & flyctl logs -f --app stockmind
```

### Deploy Forçando Rebuild

```bash
flyctl deploy --app stockmind --no-cache
```

## ?? Monitoramento

### Ver Logs em Tempo Real

```bash
flyctl logs -f --app stockmind
```

### Ver Logs Específicos

```bash
# Últimas 100 linhas
flyctl logs --app stockmind -n 100

# Filtrar por termo
flyctl logs --app stockmind | findstr "error"  # Windows
flyctl logs --app stockmind | grep "error"     # Linux/Mac
```

### Status do App

```bash
flyctl status --app stockmind
```

### Dashboard no Browser

```bash
flyctl dashboard --app stockmind
```

### Abrir App no Browser

```bash
flyctl open --app stockmind
```

## ?? Testar a Aplicação

### Health Check

```bash
curl https://stockmind.fly.dev/health
```

### API com PowerShell (Windows)

```powershell
# Health check
Invoke-RestMethod -Uri "https://stockmind.fly.dev/health"

# Login (exemplo)
$body = @{
    email = "admin@stockmind.com"
    password = "Admin@123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://stockmind.fly.dev/api/auth/login" -Method POST -Body $body -ContentType "application/json"
```

### API com curl

```bash
# Health check
curl https://stockmind.fly.dev/health

# Login
curl -X POST https://stockmind.fly.dev/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@stockmind.com","password":"Admin@123"}'
```

## ?? Escalar Recursos

### Ver Configuração Atual

```bash
flyctl scale show --app stockmind
```

### Aumentar Memória

```bash
# 512MB (padrão)
flyctl scale memory 512 --app stockmind

# 1GB (recomendado)
flyctl scale memory 1024 --app stockmind

# 2GB
flyctl scale memory 2048 --app stockmind
```

### Mudar Tipo de VM

```bash
# Shared CPU 1x (256MB)
flyctl scale vm shared-cpu-1x --app stockmind

# Shared CPU 2x (512MB)
flyctl scale vm shared-cpu-2x --app stockmind
```

### Número de Instâncias

```bash
# Ver quantas instâncias estão rodando
flyctl scale count --app stockmind

# Rodar 2 instâncias
flyctl scale count 2 --app stockmind

# Voltar para 1 instância
flyctl scale count 1 --app stockmind
```

## ?? Gerenciamento

### Reiniciar App

```bash
flyctl apps restart stockmind
```

### Ver Releases

```bash
flyctl releases --app stockmind
```

### Rollback para Versão Anterior

```bash
# Ver versões disponíveis
flyctl releases --app stockmind

# Rollback para v2
flyctl releases rollback v2 --app stockmind
```

### Acessar Console do Container

```bash
flyctl ssh console --app stockmind
```

### Executar Comando no Container

```bash
# Ver arquivos
flyctl ssh console --app stockmind -C "ls -la /app"

# Ver logs internos
flyctl ssh console --app stockmind -C "cat /app/logs/*.txt"

# Testar conectividade
flyctl ssh console --app stockmind -C "curl http://localhost:8080/health"
```

## ??? Banco de Dados

### PostgreSQL no Fly.io

```bash
# Listar bancos
flyctl postgres list

# Ver info do banco
flyctl postgres db list --app stockmind-db

# Conectar ao banco
flyctl postgres connect --app stockmind-db

# Backup
flyctl postgres backup create --app stockmind-db

# Ver backups
flyctl postgres backup list --app stockmind-db
```

### Rodar Migrations

```bash
# SSH no container e rodar migrations
flyctl ssh console --app stockmind -C "dotnet ef database update"

# Ou criar um job one-off
flyctl ssh console --app stockmind
cd /app
dotnet StockMind.API.dll --migrate
exit
```

## ?? Gerenciar Secrets

### Listar Secrets (sem valores)

```bash
flyctl secrets list --app stockmind
```

### Adicionar/Atualizar Secret

```bash
flyctl secrets set KEY=VALUE --app stockmind
```

### Remover Secret

```bash
flyctl secrets unset KEY --app stockmind
```

### Importar Múltiplos Secrets de Arquivo

```bash
# Criar arquivo .env
echo "JwtSettings__Secret=meu-secret" > .env
echo "Email__SmtpHost=smtp.gmail.com" >> .env

# Importar
flyctl secrets import --app stockmind < .env
```

## ?? Domínio Customizado

### Adicionar Certificado SSL

```bash
flyctl certs create seudominio.com --app stockmind
```

### Ver Certificados

```bash
flyctl certs list --app stockmind
```

### Ver Instruções de DNS

```bash
flyctl certs show seudominio.com --app stockmind
```

## ?? CI/CD

### Deploy via GitHub Actions

Criar `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Fly.io

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: superfly/flyctl-actions/setup-flyctl@master
      - run: flyctl deploy --app stockmind --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
```

### Gerar Token para CI/CD

```bash
flyctl auth token
```

## ?? Limpeza

### Deletar App (CUIDADO!)

```bash
flyctl apps destroy stockmind
```

### Deletar Banco (CUIDADO!)

```bash
flyctl postgres destroy stockmind-db
```

## ?? Custos e Uso

### Ver Uso Atual

```bash
flyctl dashboard --app stockmind
```

### Ver Faturamento

```bash
flyctl billing show
```

## ?? Ajuda

### Ver Todos os Comandos

```bash
flyctl help
```

### Ajuda de Comando Específico

```bash
flyctl deploy --help
flyctl secrets --help
```

## ?? Workflow Típico

```bash
# 1. Fazer mudanças no código
git add .
git commit -m "Minha feature"
git push

# 2. Deploy no Fly.io
cd C:\StockMind\StockMind
flyctl deploy --app stockmind

# 3. Verificar logs
flyctl logs -f --app stockmind

# 4. Testar
curl https://stockmind.fly.dev/health

# 5. Se tudo OK, commit final
# 6. Se houver problema, ver logs e corrigir
flyctl logs --app stockmind | findstr "ERROR"
```

## ? Comandos Quick Reference

```bash
# Deploy
flyctl deploy --app stockmind

# Logs
flyctl logs -f --app stockmind

# Status
flyctl status --app stockmind

# Restart
flyctl apps restart stockmind

# SSH
flyctl ssh console --app stockmind

# Dashboard
flyctl dashboard --app stockmind

# Secrets
flyctl secrets list --app stockmind

# Scale
flyctl scale memory 1024 --app stockmind
```

---

**?? Dica:** Salve estes comandos para referência rápida!

**?? Links:**
- Documentação completa: `DEPLOY_FLYIO.md`
- Troubleshooting: `TROUBLESHOOTING_DOCKER_FLYIO.md`
- Checklist: `DEPLOY_CHECKLIST.md`
