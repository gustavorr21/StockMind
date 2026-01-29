# ? Checklist de Deploy - StockMind no Fly.io

Use este checklist para garantir que tudo está configurado corretamente antes e durante o deploy.

## ?? Pré-Deploy

### Arquivos do Projeto
- [x] `Dockerfile` criado na raiz
- [x] `.dockerignore` criado
- [x] `fly.toml` configurado
- [x] `appsettings.Production.json` criado
- [x] Endpoint `/health` adicionado ao Program.cs
- [x] Build local funcionando ?

### Ferramentas
- [ ] Fly CLI instalado ([Download](https://fly.io/docs/hands-on/install-flyctl/))
- [ ] Docker Desktop instalado (opcional, para testes)
- [ ] Conta no Fly.io criada ([Criar conta](https://fly.io/app/sign-up))
- [ ] Login no Fly CLI realizado (`flyctl auth login`)

### Teste Local com Docker
- [ ] Build Docker funciona: `docker build -t stockmind:local .`
- [ ] Container inicia: `docker run -p 8080:8080 stockmind:local`
- [ ] Health check responde: `curl http://localhost:8080/health`
- [ ] API acessível: `http://localhost:8080/swagger`

## ?? Durante o Deploy

### Criação do App
- [ ] App criado no Fly.io: `flyctl apps create stockmind`
- [ ] Região configurada: Amsterdam (ams) ou outra
- [ ] Nome do app confirmado e disponível

### Banco de Dados (Escolha uma opção)

#### Opção A: Fly Postgres
- [ ] PostgreSQL criado: `flyctl postgres create --name stockmind-db`
- [ ] Banco anexado: `flyctl postgres attach stockmind-db --app stockmind`

#### Opção B: Banco Externo
- [ ] Connection string obtida
- [ ] Secret configurado: `flyctl secrets set ConnectionStrings__DefaultConnection="..."`

### Configuração de Secrets

#### Obrigatórios
- [ ] JWT Secret (min 32 chars): `flyctl secrets set JwtSettings__Secret="..."`

#### Opcionais
- [ ] Email SMTP configurado (se usar notificações)
- [ ] RabbitMQ configurado (se usar externo)
- [ ] Redis configurado (se usar externo)

### Deploy da Aplicação
- [ ] Deploy executado: `flyctl deploy --app stockmind`
- [ ] Build completado sem erros
- [ ] Health check passou
- [ ] App em execução

## ?? Pós-Deploy

### Verificação Básica
- [ ] Status OK: `flyctl status --app stockmind`
- [ ] Logs sem erros críticos: `flyctl logs --app stockmind`
- [ ] Health endpoint responde: `curl https://stockmind.fly.dev/health`
- [ ] Swagger acessível: `https://stockmind.fly.dev/swagger`

### Teste de Funcionalidades
- [ ] Consegue criar conta/fazer login
- [ ] Endpoints da API respondem corretamente
- [ ] Banco de dados está persistindo dados
- [ ] SignalR conecta (se aplicável)

### Configuração Adicional
- [ ] CORS configurado para frontend (se necessário)
- [ ] Domínio customizado configurado (opcional)
- [ ] SSL/HTTPS funcionando
- [ ] Backup do banco configurado

### Monitoramento
- [ ] Logs monitorados: `flyctl logs -f --app stockmind`
- [ ] Dashboard acessado: `flyctl dashboard --app stockmind`
- [ ] Métricas verificadas
- [ ] Alertas configurados (opcional)

## ?? Otimização (Opcional)

### Performance
- [ ] Memória ajustada se necessário: `flyctl scale memory 1024`
- [ ] CPU ajustado se necessário: `flyctl scale vm shared-cpu-2x`
- [ ] Múltiplas instâncias (se necessário): `flyctl scale count 2`

### Segurança
- [ ] Secrets revisados (não expor em logs)
- [ ] HTTPS enforced
- [ ] CORS configurado adequadamente
- [ ] Rate limiting configurado (se necessário)

### Custos
- [ ] Free tier suficiente? (3 VMs, 256MB cada)
- [ ] Plano adequado ao uso esperado
- [ ] Orçamento definido (se aplicável)

## ?? Troubleshooting

Se algo der errado, verifique:

- [ ] Logs detalhados: `flyctl logs --app stockmind`
- [ ] Status do app: `flyctl status --app stockmind`
- [ ] Secrets configurados: `flyctl secrets list --app stockmind`
- [ ] Console SSH: `flyctl ssh console --app stockmind`
- [ ] Documentação: `TROUBLESHOOTING_DOCKER_FLYIO.md`

## ?? Comandos Úteis

```bash
# Ver status
flyctl status --app stockmind

# Ver logs em tempo real
flyctl logs -f --app stockmind

# Reiniciar app
flyctl apps restart stockmind

# Escalar recursos
flyctl scale memory 1024 --app stockmind
flyctl scale count 2 --app stockmind

# Acessar console
flyctl ssh console --app stockmind

# Ver secrets (nomes apenas)
flyctl secrets list --app stockmind

# Configurar novo secret
flyctl secrets set KEY=value --app stockmind

# Remover secret
flyctl secrets unset KEY --app stockmind

# Ver releases
flyctl releases --app stockmind

# Rollback
flyctl releases rollback v2 --app stockmind

# Dashboard
flyctl dashboard --app stockmind

# Abrir app no browser
flyctl open --app stockmind
```

## ?? Métricas de Sucesso

### Deploy Bem-Sucedido Se:
- ? Status: `running`
- ? Health check: `passing`
- ? Response time: `< 500ms`
- ? CPU usage: `< 80%`
- ? Memory usage: `< 80%`
- ? Uptime: `> 99%`

### Sinais de Problema:
- ? Status: `pending`, `failed`, `crashed`
- ? Health check: `failing`
- ? Response time: `> 2s`
- ? Erros constantes nos logs
- ? Out of memory errors
- ? Connection timeouts

## ?? Deploy Completo!

Quando todos os itens acima estiverem marcados, seu deploy está completo e funcional!

**Próximos passos:**
1. Testar todas as funcionalidades
2. Configurar monitoramento
3. Documentar URLs e credenciais
4. Compartilhar com o time
5. Planejar manutenção e backups

---

**URLs Importantes:**
- App: `https://stockmind.fly.dev`
- Swagger: `https://stockmind.fly.dev/swagger`
- Health: `https://stockmind.fly.dev/health`
- Dashboard: `https://fly.io/dashboard/stockmind`

**Suporte:**
- Documentação: Ver arquivos `DEPLOY_*.md`
- Fly.io Docs: https://fly.io/docs/
- Community: https://community.fly.io/

Boa sorte! ??
