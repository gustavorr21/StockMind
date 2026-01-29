# ?? Troubleshooting - Docker & Fly.io

## ?? Problemas com Docker Local

### Erro: "Could not find a part of the path"

**Causa**: Paths do Windows não são compatíveis com Docker Linux.

**Solução**: O Dockerfile já está configurado corretamente com paths relativos.

### Erro: Build falha ao restaurar pacotes

```bash
# Limpar cache do Docker
docker builder prune -a

# Rebuild sem cache
docker build --no-cache -t stockmind:local .
```

### Erro: Container inicia mas não responde

**Verificar logs**:
```bash
docker logs <container-id>
```

**Verificar se a porta está correta**:
```bash
# O container expõe porta 8080
docker run -p 8080:8080 stockmind:local

# Testar
curl http://localhost:8080/health
```

### Erro: "One or more errors occurred. (Connection refused)"

**Causa**: Tentando conectar ao banco de dados localhost dentro do container.

**Solução**: Use `host.docker.internal` para conexões localhost:

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=StockMindDb;..." \
  stockmind:local
```

## ?? Problemas com Fly.io

### Erro: "Could not detect runtime or Dockerfile"

**Solução**: 
1. Certifique-se de estar na raiz do projeto (onde está o `Dockerfile`)
2. Execute: `flyctl deploy --app stockmind --dockerfile Dockerfile`

### Erro: "App name already taken"

**Solução**: 
```bash
# Use um nome diferente
flyctl apps create stockmind-seuapelido

# Ou se você já possui o app
flyctl deploy --app stockmind
```

### Erro: Health check failing

**Verificar logs**:
```bash
flyctl logs --app stockmind
```

**Verificar status**:
```bash
flyctl status --app stockmind
```

**Causas comuns**:
1. **Banco de dados não configurado**: Configure a connection string
2. **Migration pendente**: Execute migrations no console SSH
3. **Porta incorreta**: Certifique-se que o app escuta na porta 8080

**Testar manualmente**:
```bash
flyctl ssh console --app stockmind
curl http://localhost:8080/health
```

### Erro: "Error: failed to fetch an image or build from source"

**Solução**: 
```bash
# Verificar se o Dockerfile está correto
docker build -t stockmind:test .

# Se funcionar localmente, tentar novamente
flyctl deploy --app stockmind
```

### Erro: "Out of memory" ou "OOMKilled"

**Solução**: Aumentar memória do container
```bash
# Ver configuração atual
flyctl scale show --app stockmind

# Aumentar para 1GB
flyctl scale memory 1024 --app stockmind

# Ou 2GB
flyctl scale memory 2048 --app stockmind
```

### Erro: Database connection timeout

**Verificar connection string**:
```bash
flyctl secrets list --app stockmind
```

**Se usar Fly Postgres**:
```bash
# Verificar se está anexado
flyctl postgres list

# Anexar se necessário
flyctl postgres attach stockmind-db --app stockmind
```

**Para Postgres externo**:
```bash
# Configurar connection string
flyctl secrets set ConnectionStrings__DefaultConnection="Host=your-host;Database=stockmind;..." --app stockmind
```

### Erro: "dial tcp: lookup failed"

**Causa**: Problema de DNS ou rede interna do Fly.io.

**Solução**:
1. Verificar se o serviço externo está acessível
2. Para serviços internos do Fly, use o hostname `.internal`
3. Exemplo: `stockmind-db.internal` ao invés de `stockmind-db.fly.dev`

### Erro: SignalR WebSocket não conecta

**Verificar CORS**:
```csharp
// No Program.cs, certifique-se que está configurado para produção
policy.WithOrigins("https://seu-frontend.com")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials(); // IMPORTANTE para SignalR
```

**Verificar URL do cliente**:
```javascript
// Use wss:// em produção
const connection = new signalR.HubConnectionBuilder()
    .withUrl("wss://stockmind.fly.dev/hubs/stock-alerts", {
        accessTokenFactory: () => yourToken
    })
    .build();
```

### Erro: "Region not found"

**Listar regiões disponíveis**:
```bash
flyctl platform regions
```

**Usar região próxima**:
```bash
# Amsterdam (Europa)
flyctl apps create stockmind --region ams

# São Paulo (Brasil) - se disponível
flyctl apps create stockmind --region gru
```

## ?? Debug Avançado

### Acessar container em execução

```bash
# SSH no container
flyctl ssh console --app stockmind

# Verificar variáveis de ambiente
printenv | grep -i connection

# Verificar logs internos
cat /app/logs/*.txt

# Testar conectividade
curl http://localhost:8080/health
ping host.com
```

### Verificar configuração do Fly

```bash
# Ver toda configuração
flyctl config show --app stockmind

# Ver secrets (nomes, não valores)
flyctl secrets list --app stockmind

# Ver volumes
flyctl volumes list --app stockmind
```

### Analisar deploy anterior

```bash
# Ver histórico de releases
flyctl releases --app stockmind

# Rollback para versão anterior
flyctl releases rollback v2 --app stockmind
```

## ?? Performance

### App muito lento

**Verificar recursos**:
```bash
flyctl status --app stockmind
flyctl scale show --app stockmind
```

**Otimizações**:
```bash
# Aumentar CPU/RAM
flyctl scale vm shared-cpu-2x --app stockmind
flyctl scale memory 1024 --app stockmind

# Adicionar mais instâncias
flyctl scale count 2 --app stockmind

# Habilitar auto-scaling
# Edite fly.toml:
# auto_start_machines = true
# auto_stop_machines = true
# min_machines_running = 1
```

### Logs muito verbosos

**Ajustar nível de log**:
```bash
flyctl secrets set Serilog__MinimumLevel__Default=Warning --app stockmind
```

## ?? Ainda com problemas?

1. **Verificar status do Fly.io**: https://status.fly.io/
2. **Community Forum**: https://community.fly.io/
3. **Logs detalhados**: `flyctl logs --app stockmind -f` (seguir em tempo real)
4. **Documentação oficial**: https://fly.io/docs/

## ?? Checklist de Diagnóstico

- [ ] Dockerfile existe na raiz do projeto?
- [ ] Build local funciona? `docker build -t test .`
- [ ] Health check responde localmente? `curl http://localhost:8080/health`
- [ ] Secrets configurados? `flyctl secrets list`
- [ ] Banco de dados configurado e acessível?
- [ ] Região do Fly.io válida?
- [ ] Memória suficiente? (mínimo 512MB)
- [ ] Logs verificados? `flyctl logs`
- [ ] Status do app OK? `flyctl status`

## ?? Dicas Úteis

- Sempre teste o build Docker localmente primeiro
- Use `.dockerignore` para otimizar builds
- Monitore os logs durante o deploy
- Configure health checks adequadamente
- Use secrets para dados sensíveis, nunca no código
- Teste a aplicação depois de cada deploy

## ?? Links Úteis

- [Fly.io Documentation](https://fly.io/docs/)
- [Docker Documentation](https://docs.docker.com/)
- [ASP.NET Core in Docker](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
