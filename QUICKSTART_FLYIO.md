# ?? Quick Start - Deploy to Fly.io

## ? Fastest Way to Deploy

### Option 1: Using PowerShell Script (Windows)

```powershell
# Execute the deployment script
.\deploy-flyio.ps1
```

### Option 2: Using Bash Script (Linux/Mac)

```bash
# Make script executable
chmod +x deploy-flyio.sh

# Execute the deployment script
./deploy-flyio.sh
```

### Option 3: Manual Deployment

```bash
# 1. Login to Fly.io
flyctl auth login

# 2. Create and deploy in one command
flyctl launch --name stockmind --region ams

# 3. Follow the prompts
```

## ?? Required Environment Variables

Before deploying, make sure to set these secrets:

```bash
# JWT Configuration (REQUIRED)
flyctl secrets set JwtSettings__Secret="your-super-secret-key-min-32-chars" --app stockmind

# Database (if not using Fly Postgres)
flyctl secrets set ConnectionStrings__DefaultConnection="your-connection-string" --app stockmind
```

## ? Verify Deployment

After deployment:

```bash
# Check if app is running
flyctl status --app stockmind

# View logs
flyctl logs --app stockmind

# Test health endpoint
curl https://stockmind.fly.dev/health
```

## ?? More Information

For detailed instructions, see [DEPLOY_FLYIO.md](./DEPLOY_FLYIO.md)

## ?? Common Issues

### Issue: "Could not detect runtime or Dockerfile"
**Solution**: Make sure you're in the project root directory where `Dockerfile` is located.

### Issue: "App already exists"
**Solution**: Use `flyctl deploy --app stockmind` instead of `flyctl launch`

### Issue: Health check failing
**Solution**: Check logs with `flyctl logs --app stockmind` and ensure database connection is configured.

## ?? Tips

- Use `flyctl ssh console --app stockmind` to access the running container
- View metrics at https://fly.io/dashboard/stockmind
- Scale up if needed: `flyctl scale memory 1024 --app stockmind`
