#!/bin/bash

# StockMind - Fly.io Deployment Script
# This script helps deploy StockMind to Fly.io

set -e

echo "?? StockMind Fly.io Deployment Helper"
echo "======================================"
echo ""

# Check if flyctl is installed
if ! command -v flyctl &> /dev/null; then
    echo "? flyctl is not installed"
    echo "?? Install it from: https://fly.io/docs/hands-on/install-flyctl/"
    exit 1
fi

# Check if user is logged in
if ! flyctl auth whoami &> /dev/null; then
    echo "?? You need to login to Fly.io"
    flyctl auth login
fi

APP_NAME="stockmind"
REGION="ams"

echo "?? Configuration:"
echo "   App Name: $APP_NAME"
echo "   Region: $REGION (Amsterdam)"
echo ""

# Check if app exists
if flyctl apps list | grep -q "^$APP_NAME"; then
    echo "? App '$APP_NAME' already exists"
    echo ""
    read -p "?? Do you want to deploy a new version? (y/n) " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "?? Deploying..."
        flyctl deploy --app $APP_NAME
        echo ""
        echo "? Deployment complete!"
        echo "?? Your app is available at: https://$APP_NAME.fly.dev"
    fi
else
    echo "??  App '$APP_NAME' does not exist"
    echo ""
    read -p "?? Do you want to create it? (y/n) " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo ""
        echo "?? Creating app..."
        flyctl apps create $APP_NAME --org personal
        echo ""
        
        read -p "???  Do you want to create a PostgreSQL database? (y/n) " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            echo "?? Creating PostgreSQL database..."
            flyctl postgres create --name ${APP_NAME}-db --region $REGION
            echo ""
            flyctl postgres attach ${APP_NAME}-db --app $APP_NAME
        fi
        
        echo ""
        echo "?? Setting up secrets..."
        echo "Please provide the following information:"
        echo ""
        
        read -p "JWT Secret (min 32 chars): " JWT_SECRET
        flyctl secrets set JwtSettings__Secret="$JWT_SECRET" --app $APP_NAME
        
        echo ""
        read -p "Do you want to configure email? (y/n) " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            read -p "SMTP Host: " SMTP_HOST
            read -p "SMTP Port: " SMTP_PORT
            read -p "From Email: " FROM_EMAIL
            read -sp "Email Password: " EMAIL_PASSWORD
            echo ""
            
            flyctl secrets set Email__SmtpHost="$SMTP_HOST" --app $APP_NAME
            flyctl secrets set Email__SmtpPort="$SMTP_PORT" --app $APP_NAME
            flyctl secrets set Email__FromEmail="$FROM_EMAIL" --app $APP_NAME
            flyctl secrets set Email__Password="$EMAIL_PASSWORD" --app $APP_NAME
        fi
        
        echo ""
        echo "?? Deploying application..."
        flyctl deploy --app $APP_NAME
        
        echo ""
        echo "? Setup complete!"
        echo "?? Your app is available at: https://$APP_NAME.fly.dev"
        echo "?? View logs: flyctl logs --app $APP_NAME"
        echo "?? Dashboard: flyctl dashboard --app $APP_NAME"
    fi
fi

echo ""
echo "?? Done!"
