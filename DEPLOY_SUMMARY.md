# ? Arquivos Criados para Deploy no Fly.io

## ?? Arquivos Adicionados

### 1. **Dockerfile** (raiz do projeto)
   - ? Multi-stage build otimizado
   - ? Usa .NET 8 SDK e Runtime
   - ? Health check configurado
   - ? Non-root user para segurança
   - ? Expõe porta 8080

### 2. **.dockerignore** (raiz do projeto)
   - ? Exclui arquivos desnecessários do build
   - ? Otimiza tamanho da imagem
   - ? Reduz tempo de build

### 3. **fly.toml** (raiz do projeto)
   - ? Configuração do Fly.io
   - ? Health check endpoint
   - ? Região: Amsterdam (ams)
   - ? 512MB RAM, auto-scaling

### 4. **src/StockMind.API/Program.cs** (modificado)
   - ? Adicionado endpoint `/health`
   - ? Retorna JSON com status

### 5. **src/StockMind.API/appsettings.Production.json** (novo)
   - ? Configurações otimizadas para produção
   - ? Logs apenas para console
   - ? Placeholders para secrets

### 6. **deploy-flyio.ps1** (script PowerShell)
   - ? Script automatizado para Windows
   - ? Cria app, configura secrets, faz deploy

### 7. **deploy-flyio.sh** (script Bash)
   - ? Script automatizado para Linux/Mac
   - ? Mesmas funcionalidades do PowerShell

### 8. **DEPLOY_FLYIO.md** (documentação completa)
   - ? Guia passo a passo detalhado
   - ? Configuração de banco de dados
   - ? Variáveis de ambiente
   - ? Comandos úteis

### 9. **QUICKSTART_FLYIO.md** (início rápido)
   - ? 3 opções de deploy
   - ? Comandos essenciais
   - ? Troubleshooting básico

### 10. **TROUBLESHOOTING_DOCKER_FLYIO.md**
   - ? Problemas comuns e soluções
   - ? Debug avançado
   - ? Performance tips

### 11. **README.md** (atualizado)
   - ? Seção de deploy adicionada
   - ? Links para documentação
   - ? Estrutura do projeto

## ?? Próximos Passos

### 1. Testar Build Local (Recomendado)

```bash
# Testar o build do Docker
docker build -t stockmind:local .

# Se der erro, verifique os logs e corrija
```

### 2. Preparar para Deploy

```bash
# Instalar Fly CLI se ainda não tiver
# Windows (PowerShell):
iwr https://fly.io/install.ps1 -useb | iex

# Mac/Linux:
curl -L https://fly.io/install.sh | sh

# Fazer login
flyctl auth login
```

### 3. Deploy - Escolha uma opção:

#### Opção A: Usando Script (Mais Fácil)

**Windows:**
```powershell
.\deploy-flyio.ps1
```

**Linux/Mac:**
```bash
chmod +x deploy-flyio.sh
./deploy-flyio.sh
```

#### Opção B: Deploy Manual

```bash
# Criar app
flyctl apps create stockmind --org personal

# (Opcional) Criar banco PostgreSQL
flyctl postgres create --name stockmind-db --region ams
flyctl postgres attach stockmind-db --app stockmind

# Configurar secrets OBRIGATÓRIOS
flyctl secrets set JwtSettings__Secret="your-super-secret-key-minimum-32-characters-long" --app stockmind

# Deploy
flyctl deploy --app stockmind
```

#### Opção C: Deploy Automático

```bash
# Fly.io detecta automaticamente e pergunta tudo
flyctl launch --name stockmind --region ams
```

### 4. Verificar Deploy

```bash
# Ver status
flyctl status --app stockmind

# Ver logs
flyctl logs --app stockmind

# Testar health check
curl https://stockmind.fly.dev/health

# Abrir no browser
flyctl open --app stockmind
```

## ?? Configurações Importantes

### Secrets Obrigatórios

```bash
# JWT (OBRIGATÓRIO para autenticação funcionar)
flyctl secrets set JwtSettings__Secret="sua-chave-secreta-min-32-chars" --app stockmind
```

### Secrets Opcionais (mas recomendados)

```bash
# Banco de dados (se não usar Fly Postgres)
flyctl secrets set ConnectionStrings__DefaultConnection="sua-connection-string" --app stockmind

# Email (para notificações)
flyctl secrets set Email__SmtpHost="smtp.gmail.com" --app stockmind
flyctl secrets set Email__SmtpPort="587" --app stockmind
flyctl secrets set Email__FromEmail="seu-email@gmail.com" --app stockmind
flyctl secrets set Email__Password="sua-senha-app" --app stockmind

# RabbitMQ (se usar externo)
flyctl secrets set RabbitMQ__HostName="seu-rabbitmq-host" --app stockmind
flyctl secrets set RabbitMQ__UserName="username" --app stockmind
flyctl secrets set RabbitMQ__Password="password" --app stockmind

# Redis (se usar externo)
flyctl secrets set Redis__Configuration="redis-host:6379" --app stockmind
```

## ?? O Que Esperar

### Durante o Deploy (5-10 minutos):

1. ? Upload do código para Fly.io
2. ? Build da imagem Docker (primeira vez é mais lento)
3. ? Deploy da imagem
4. ? Health check verificado
5. ? App disponível em `https://stockmind.fly.dev`

### Após o Deploy:

- **API**: `https://stockmind.fly.dev`
- **Swagger**: `https://stockmind.fly.dev/swagger`
- **Health**: `https://stockmind.fly.dev/health`
- **SignalR**: `wss://stockmind.fly.dev/hubs/stock-alerts`

## ?? Problemas Comuns

### "Could not detect runtime or Dockerfile"
? **Resolvido!** O Dockerfile agora está na raiz do projeto.

### Build falha
```bash
# Testar localmente primeiro
docker build -t test .

# Ver logs detalhados
flyctl logs --app stockmind
```

### Health check failing
```bash
# Verificar se o banco está configurado
flyctl secrets list --app stockmind

# Ver logs
flyctl logs --app stockmind

# Acessar console
flyctl ssh console --app stockmind
curl http://localhost:8080/health
```

### App lento
```bash
# Aumentar recursos
flyctl scale memory 1024 --app stockmind
flyctl scale vm shared-cpu-2x --app stockmind
```

## ?? Documentação

- **Deploy Completo**: [DEPLOY_FLYIO.md](./DEPLOY_FLYIO.md)
- **Quick Start**: [QUICKSTART_FLYIO.md](./QUICKSTART_FLYIO.md)
- **Troubleshooting**: [TROUBLESHOOTING_DOCKER_FLYIO.md](./TROUBLESHOOTING_DOCKER_FLYIO.md)
- **Fly.io Docs**: https://fly.io/docs/

## ? Recursos do Fly.io Free Tier

- ? 3 máquinas compartilhadas (256MB cada)
- ? 3GB de storage persistente
- ? 160GB de tráfego/mês
- ? SSL/HTTPS automático
- ? Certificados gerenciados

**Para StockMind**, recomendo:
- **Plano**: Hobby ($5-10/mês) ou Free tier para testes
- **Memória**: 512MB-1GB
- **Região**: Amsterdam (ams) ou próxima aos usuários

## ?? Pronto!

Agora você tem tudo para fazer deploy no Fly.io!

Execute:
```bash
flyctl launch --name stockmind --region ams
```

E siga os prompts. Boa sorte! ??

## ?? Dicas Finais

1. **Sempre teste localmente primeiro** com Docker
2. **Configure os secrets antes do primeiro deploy**
3. **Monitore os logs** durante e após o deploy
4. **Use o Free tier** para testes, upgrade depois se necessário
5. **Faça backup** do banco de dados regularmente

## ?? Suporte

- Issues no GitHub: [criar issue](https://github.com/gustavorr21/StockMind/issues)
- Fly.io Community: https://community.fly.io/
- Documentação: Ver arquivos DEPLOY_*.md

---

**Criado por GitHub Copilot** ??
**Data**: 2024
