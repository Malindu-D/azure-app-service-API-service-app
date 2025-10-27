#!/bin/bash

# Azure App Service deployment script
# This script creates Azure resources and deploys the API

# Variables - Update these with your values
RESOURCE_GROUP="rg-userdata-api"
LOCATION="eastus"
APP_SERVICE_PLAN="asp-userdata-api"
APP_SERVICE_NAME="app-userdata-api-${RANDOM}"
SERVICE_BUS_NAMESPACE="sb-userdata-${RANDOM}"
QUEUE_NAME="userdata-queue"

echo "Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

echo "Creating Service Bus namespace..."
az servicebus namespace create \
  --resource-group $RESOURCE_GROUP \
  --name $SERVICE_BUS_NAMESPACE \
  --location $LOCATION \
  --sku Basic

echo "Creating Service Bus queue..."
az servicebus queue create \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $SERVICE_BUS_NAMESPACE \
  --name $QUEUE_NAME

echo "Creating App Service Plan..."
az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku B1 \
  --is-linux

echo "Creating App Service..."
az webapp create \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --name $APP_SERVICE_NAME \
  --runtime "DOTNETCORE:8.0"

echo "Enabling Managed Identity..."
az webapp identity assign \
  --name $APP_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP

# Get the principal ID of the managed identity
PRINCIPAL_ID=$(az webapp identity show \
  --name $APP_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --query principalId -o tsv)

echo "Granting Service Bus Data Sender role to Managed Identity..."
SERVICE_BUS_ID=$(az servicebus namespace show \
  --resource-group $RESOURCE_GROUP \
  --name $SERVICE_BUS_NAMESPACE \
  --query id -o tsv)

az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "Azure Service Bus Data Sender" \
  --scope $SERVICE_BUS_ID

echo "Configuring App Service settings..."
az webapp config appsettings set \
  --name $APP_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ServiceBus__Namespace="${SERVICE_BUS_NAMESPACE}.servicebus.windows.net" \
    ServiceBus__QueueOrTopicName="$QUEUE_NAME" \
    Cors__AllowedOrigins__0="https://your-static-web-app.azurestaticapps.net"

echo "Deploying application..."
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
cd ..

az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $APP_SERVICE_NAME \
  --src app.zip

echo "Deployment complete!"
echo "App Service URL: https://${APP_SERVICE_NAME}.azurewebsites.net"
echo "Service Bus Namespace: ${SERVICE_BUS_NAMESPACE}.servicebus.windows.net"
echo "Queue Name: $QUEUE_NAME"
