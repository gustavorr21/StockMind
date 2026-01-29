# ?? Deploy StockMind no Fly.io

Este guia explica como fazer o deploy da aplicação StockMind no Fly.io usando Docker.

## ?? Pré-requisitos

1. **Conta no Fly.io**
   - Acesse [fly.io](https://fly.io) e crie uma conta
   - Instale o Fly CLI: https://fly.io/docs/hands-on/install-flyctl/

2. **Docker Desktop** (opcional, para testes locais)
   - Download: https://www.docker.com/products/docker-desktop

3. **Autenticação no Fly.io**
   ```bash
   flyctl auth login
   ```

## ?? Configuração Inicial

### 1. Criar a aplicação no Fly.io

```bash
# Na raiz do projeto
flyctl apps create stockmind
```

### 2. Configurar Banco de Dados PostgreSQL (Opcional)

Se você quer usar PostgreSQL gerenciado pelo Fly.io:

```bash
# Criar banco PostgreSQL
flyctl postgres create --name stockmind-db --region ams

# Conectar o banco à aplicação
flyctl postgres attach stockmind-db --app stockmind
```

Isso irá automaticamente configurar a variável de ambiente `DATABASE_URL`.

### 3. Configurar Variáveis de Ambiente

Edite o arquivo `fly.toml` ou use comandos para adicionar secrets:

```bash
# JWT Settings
flyctl secrets set JwtSettings__Secret="your-super-secret-key-min-32-chars" --app stockmind
flyctl secrets set JwtSettings__Issuer="StockMind" --app stockmind
flyctl secrets set JwtSettings__Audience="StockMindUsers" --app stockmind

# Database (se não usar Fly Postgres)
flyctl secrets set ConnectionStrings__DefaultConnection="Host=your-host;Database=stockmind;Username=user;Password=pass" --app stockmind

# Redis (se necessário)
flyctl secrets set Redis__ConnectionString="your-redis-host:6379" --app stockmind

# RabbitMQ (se necessário)
flyctl secrets set RabbitMQ__HostName="your-rabbitmq-host" --app stockmind
flyctl secrets set RabbitMQ__UserName="your-username" --app stockmind
flyctl secrets set RabbitMQ__Password="your-password" --app stockmind

# Email (se necessário)
flyctl secrets set Email__SmtpHost="smtp.gmail.com" --app stockmind
flyctl secrets set Email__SmtpPort="587" --app stockmind
flyctl secrets set Email__FromEmail="your-email@gmail.com" --app stockmind
flyctl secrets set Email__Password="your-app-password" --app stockmind
```

## ?? Deploy

### Deploy usando Dockerfile

```bash
# Na raiz do projeto (onde está o Dockerfile)
flyctl deploy --app stockmind
```

### Primeira Deploy com Configuração Automática

```bash
# Se quiser que o Fly.io configure tudo automaticamente
flyctl launch --name stockmind --region ams
```

## ?? Monitoramento e Logs

### Ver logs em tempo real
```bash
flyctl logs --app stockmind
```

### Ver status da aplicação
```bash
flyctl status --app stockmind
```

### Abrir a aplicação no navegador
```bash
flyctl open --app stockmind
```

### Ver métricas
```bash
flyctl dashboard --app stockmind
```

## ?? Testar Localmente com Docker

Antes de fazer deploy, teste localmente:

```bash
# Build da imagem
docker build -t stockmind:local .

# Executar container
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="sua-connection-string" \
  stockmind:local

# Testar health check
curl http://localhost:8080/health
```

## ?? Atualizar a Aplicação

Após fazer alterações no código:

```bash
# Commit suas alterações
git add .
git commit -m "sua mensagem"

# Deploy nova versão
flyctl deploy --app stockmind
```

## ?? Configurar Domínio Customizado (Opcional)

```bash
# Adicionar certificado SSL
flyctl certs create yourdomain.com --app stockmind

# Configurar DNS
# Adicione um registro CNAME apontando para stockmind.fly.dev
```

## ?? Escalar a Aplicação

```bash
# Ver configuração atual
flyctl scale show --app stockmind

# Aumentar memória
flyctl scale memory 1024 --app stockmind

# Adicionar mais instâncias
flyctl scale count 2 --app stockmind
```

## ?? Troubleshooting

### Erro: "Could not detect runtime or Dockerfile"
? **Solução**: O Dockerfile agora está na raiz do projeto.

### Erro de conexão com banco de dados
- Verifique se as variáveis de ambiente estão configuradas:
  ```bash
  flyctl secrets list --app stockmind
  ```
- Teste a conexão manualmente com `flyctl ssh console`

### Aplicação não inicia
- Verifique os logs: `flyctl logs --app stockmind`
- Verifique o health check: `curl https://stockmind.fly.dev/health`
- Verifique as configurações: `flyctl config show --app stockmind`

### WebSocket (SignalR) não funciona
- Certifique-se que o CORS está configurado corretamente
- Verifique se o cliente está usando `wss://` (WebSocket Secure) em produção

## ?? Checklist de Deploy

- [ ] Dockerfile criado na raiz do projeto
- [ ] .dockerignore configurado
- [ ] fly.toml configurado
- [ ] Secrets configurados (JWT, Database, etc.)
- [ ] Banco de dados criado (se usando Fly Postgres)
- [ ] Health check endpoint funcionando
- [ ] Build local testado
- [ ] Deploy realizado com sucesso
- [ ] Logs verificados
- [ ] Aplicação acessível via browser

## ?? Links Úteis

- [Fly.io Documentation](https://fly.io/docs/)
- [Fly.io .NET Guide](https://fly.io/docs/languages-and-frameworks/dotnet/)
- [Fly.io Postgres](https://fly.io/docs/postgres/)
- [Fly.io Pricing](https://fly.io/docs/about/pricing/)

## ?? Custos

O Fly.io oferece um free tier generoso:
- 3 máquinas compartilhadas (256MB RAM cada)
- 3GB de armazenamento persistente
- 160GB de tráfego de saída por mês

Para mais informações, visite: https://fly.io/docs/about/pricing/

## ?? Suporte

Em caso de problemas:
1. Verifique os logs: `flyctl logs`
2. Consulte a documentação: https://fly.io/docs/
3. Community forum: https://community.fly.io/
