# Azure App Service deployment script (PowerShell)
# This script creates Azure resources and deploys the API

# Variables - Update these with your values
$RESOURCE_GROUP = "rg-userdata-api"
$LOCATION = "eastus"
$APP_SERVICE_PLAN = "asp-userdata-api"
$APP_SERVICE_NAME = "app-userdata-api-$(Get-Random)"
$SERVICE_BUS_NAMESPACE = "sb-userdata-$(Get-Random)"
$USERDATA_QUEUE_NAME = "userdata-queue"
$EMAIL_EXPORT_APP_URL = "https://your-email-export-app.azurewebsites.net"  # Update with your email export app URL

Write-Host "Creating resource group..." -ForegroundColor Green
az group create --name $RESOURCE_GROUP --location $LOCATION

Write-Host "Creating Service Bus namespace..." -ForegroundColor Green
az servicebus namespace create `
  --resource-group $RESOURCE_GROUP `
  --name $SERVICE_BUS_NAMESPACE `
  --location $LOCATION `
  --sku Basic

Write-Host "Creating Service Bus queue..." -ForegroundColor Green
az servicebus queue create `
  --resource-group $RESOURCE_GROUP `
  --namespace-name $SERVICE_BUS_NAMESPACE `
  --name $USERDATA_QUEUE_NAME

Write-Host "Creating App Service Plan..." -ForegroundColor Green
az appservice plan create `
  --name $APP_SERVICE_PLAN `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION `
  --sku B1 `
  --is-linux

Write-Host "Creating App Service..." -ForegroundColor Green
az webapp create `
  --resource-group $RESOURCE_GROUP `
  --plan $APP_SERVICE_PLAN `
  --name $APP_SERVICE_NAME `
  --runtime "DOTNETCORE:8.0"

Write-Host "Enabling Managed Identity..." -ForegroundColor Green
az webapp identity assign `
  --name $APP_SERVICE_NAME `
  --resource-group $RESOURCE_GROUP

# Get the principal ID of the managed identity
$PRINCIPAL_ID = az webapp identity show `
  --name $APP_SERVICE_NAME `
  --resource-group $RESOURCE_GROUP `
  --query principalId -o tsv

Write-Host "Granting Service Bus Data Sender role to Managed Identity..." -ForegroundColor Green
$SERVICE_BUS_ID = az servicebus namespace show `
  --resource-group $RESOURCE_GROUP `
  --name $SERVICE_BUS_NAMESPACE `
  --query id -o tsv

az role assignment create `
  --assignee $PRINCIPAL_ID `
  --role "Azure Service Bus Data Sender" `
  --scope $SERVICE_BUS_ID

Write-Host "Configuring App Service settings..." -ForegroundColor Green
az webapp config appsettings set `
  --name $APP_SERVICE_NAME `
  --resource-group $RESOURCE_GROUP `
  --settings `
    "ServiceBus__Namespace=$SERVICE_BUS_NAMESPACE.servicebus.windows.net" `
    "ServiceBus__UserDataQueue=$USERDATA_QUEUE_NAME" `
    "ServiceBus__QueueOrTopicName=$USERDATA_QUEUE_NAME" `
    "EmailExportApp__Url=$EMAIL_EXPORT_APP_URL" `
    "Cors__AllowedOrigins__0=https://your-static-web-app.azurestaticapps.net"

Write-Host "Publishing application..." -ForegroundColor Green
dotnet publish -c Release -o ./publish

Write-Host "Creating deployment package..." -ForegroundColor Green
Compress-Archive -Path ./publish/* -DestinationPath app.zip -Force

Write-Host "Deploying to App Service..." -ForegroundColor Green
az webapp deployment source config-zip `
  --resource-group $RESOURCE_GROUP `
  --name $APP_SERVICE_NAME `
  --src app.zip

Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host "App Service URL: https://$APP_SERVICE_NAME.azurewebsites.net" -ForegroundColor Yellow
Write-Host "Service Bus Namespace: $SERVICE_BUS_NAMESPACE.servicebus.windows.net" -ForegroundColor Yellow
Write-Host "User Data Queue: $USERDATA_QUEUE_NAME" -ForegroundColor Yellow
Write-Host "" -ForegroundColor Yellow
Write-Host "API Endpoints:" -ForegroundColor Cyan
Write-Host "  - User Data: https://$APP_SERVICE_NAME.azurewebsites.net/api/userdata" -ForegroundColor White
Write-Host "  - Notification: https://$APP_SERVICE_NAME.azurewebsites.net/api/notification" -ForegroundColor White
Write-Host "" -ForegroundColor Yellow
Write-Host "⚠️  IMPORTANT: Update EmailExportApp__Url in App Service configuration with your actual email export app URL" -ForegroundColor Yellow
