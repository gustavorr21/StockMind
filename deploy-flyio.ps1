# StockMind - Fly.io Deployment Script (PowerShell)
# This script helps deploy StockMind to Fly.io

$ErrorActionPreference = "Stop"

Write-Host "?? StockMind Fly.io Deployment Helper" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if flyctl is installed
if (-not (Get-Command flyctl -ErrorAction SilentlyContinue)) {
    Write-Host "? flyctl is not installed" -ForegroundColor Red
    Write-Host "?? Install it from: https://fly.io/docs/hands-on/install-flyctl/" -ForegroundColor Yellow
    exit 1
}

# Check if user is logged in
try {
    flyctl auth whoami | Out-Null
} catch {
    Write-Host "?? You need to login to Fly.io" -ForegroundColor Yellow
    flyctl auth login
}

$APP_NAME = "stockmind"
$REGION = "ams"

Write-Host "?? Configuration:" -ForegroundColor Green
Write-Host "   App Name: $APP_NAME"
Write-Host "   Region: $REGION (Amsterdam)"
Write-Host ""

# Check if app exists
$appExists = (flyctl apps list | Select-String "^$APP_NAME")

if ($appExists) {
    Write-Host "? App '$APP_NAME' already exists" -ForegroundColor Green
    Write-Host ""
    $deploy = Read-Host "?? Do you want to deploy a new version? (y/n)"
    
    if ($deploy -eq 'y' -or $deploy -eq 'Y') {
        Write-Host "?? Deploying..." -ForegroundColor Cyan
        flyctl deploy --app $APP_NAME
        Write-Host ""
        Write-Host "? Deployment complete!" -ForegroundColor Green
        Write-Host "?? Your app is available at: https://$APP_NAME.fly.dev" -ForegroundColor Cyan
    }
} else {
    Write-Host "??  App '$APP_NAME' does not exist" -ForegroundColor Yellow
    Write-Host ""
    $create = Read-Host "?? Do you want to create it? (y/n)"
    
    if ($create -eq 'y' -or $create -eq 'Y') {
        Write-Host ""
        Write-Host "?? Creating app..." -ForegroundColor Cyan
        flyctl apps create $APP_NAME --org personal
        Write-Host ""
        
        $createDb = Read-Host "???  Do you want to create a PostgreSQL database? (y/n)"
        if ($createDb -eq 'y' -or $createDb -eq 'Y') {
            Write-Host "?? Creating PostgreSQL database..." -ForegroundColor Cyan
            flyctl postgres create --name "$APP_NAME-db" --region $REGION
            Write-Host ""
            flyctl postgres attach "$APP_NAME-db" --app $APP_NAME
        }
        
        Write-Host ""
        Write-Host "?? Setting up secrets..." -ForegroundColor Cyan
        Write-Host "Please provide the following information:" -ForegroundColor Yellow
        Write-Host ""
        
        $jwtSecret = Read-Host "JWT Secret (min 32 chars)"
        flyctl secrets set "JwtSettings__Secret=$jwtSecret" --app $APP_NAME
        
        Write-Host ""
        $configEmail = Read-Host "Do you want to configure email? (y/n)"
        if ($configEmail -eq 'y' -or $configEmail -eq 'Y') {
            $smtpHost = Read-Host "SMTP Host"
            $smtpPort = Read-Host "SMTP Port"
            $fromEmail = Read-Host "From Email"
            $emailPassword = Read-Host "Email Password" -AsSecureString
            $emailPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                [Runtime.InteropServices.Marshal]::SecureStringToBSTR($emailPassword))
            
            flyctl secrets set "Email__SmtpHost=$smtpHost" --app $APP_NAME
            flyctl secrets set "Email__SmtpPort=$smtpPort" --app $APP_NAME
            flyctl secrets set "Email__FromEmail=$fromEmail" --app $APP_NAME
            flyctl secrets set "Email__Password=$emailPasswordPlain" --app $APP_NAME
        }
        
        Write-Host ""
        Write-Host "?? Deploying application..." -ForegroundColor Cyan
        flyctl deploy --app $APP_NAME
        
        Write-Host ""
        Write-Host "? Setup complete!" -ForegroundColor Green
        Write-Host "?? Your app is available at: https://$APP_NAME.fly.dev" -ForegroundColor Cyan
        Write-Host "?? View logs: flyctl logs --app $APP_NAME" -ForegroundColor Yellow
        Write-Host "?? Dashboard: flyctl dashboard --app $APP_NAME" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "?? Done!" -ForegroundColor Green
