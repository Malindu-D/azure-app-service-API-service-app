# 🚀 Quick Start Guide

Get your User Data API running in 5 minutes!

## Local Development Quick Start

### 1. Prerequisites Check

```powershell
# Check .NET SDK
dotnet --version  # Should be 8.0 or higher

# Check Azure CLI
az --version
```

### 2. Restore and Build

```powershell
cd c:\Users\malin\OneDrive\Desktop\test-api-service-app
dotnet restore
dotnet build
```

### 3. Run Locally (Optional - requires Service Bus)

```powershell
# Update appsettings.Development.json with your Service Bus details first
dotnet run
```

The API will start at `https://localhost:7001` (check console for actual port)

### 4. Test with Example HTML

Open `example-static-web-app.html` in your browser to test the API.

## Azure Deployment Quick Start

### Option 1: Automated Deployment (Recommended)

```powershell
# 1. Login to Azure
az login

# 2. Set subscription (if needed)
az account set --subscription "Your-Subscription-Name"

# 3. Run deployment script
.\deploy.ps1
```

The script will:

- ✅ Create all Azure resources
- ✅ Configure Managed Identity
- ✅ Set up Service Bus permissions
- ✅ Deploy the application

**Estimated time:** 5-10 minutes

### Option 2: Using Azure Developer CLI

```powershell
# Install azd (if not installed)
winget install microsoft.azd

# Login and deploy
azd auth login
azd up
```

### Option 3: Manual Azure Portal Deployment

1. **Create App Service**

   - Go to Azure Portal → Create App Service
   - Runtime: .NET 8
   - OS: Linux or Windows
   - Enable System Managed Identity

2. **Create Service Bus**

   - Create Service Bus Namespace (Standard tier)
   - Create Queue named `userdata-queue`
   - Grant App Service's Managed Identity "Azure Service Bus Data Sender" role

3. **Configure App Service**

   - Add Configuration → Application Settings:
     - `ServiceBus__Namespace` = `your-sb.servicebus.windows.net`
     - `ServiceBus__QueueOrTopicName` = `userdata-queue`
     - `Cors__AllowedOrigins__0` = `https://your-static-web-app.azurestaticapps.net`

4. **Deploy Code**

   ```powershell
   dotnet publish -c Release -o ./publish
   Compress-Archive -Path ./publish/* -DestinationPath app.zip -Force

   az webapp deployment source config-zip `
     --resource-group YOUR_RG `
     --name YOUR_APP_NAME `
     --src app.zip
   ```

## Post-Deployment Steps

### 1. Update Static Web App

Update the API URL in your static web app to point to your deployed App Service:

```javascript
const apiUrl = "https://YOUR-APP-SERVICE.azurewebsites.net/api/userdata";
```

### 2. Test the Deployment

```powershell
# Health check
curl https://YOUR-APP-SERVICE.azurewebsites.net/health

# Submit test data
$body = @{name="Test User"; age=25} | ConvertTo-Json
Invoke-RestMethod -Uri "https://YOUR-APP-SERVICE.azurewebsites.net/api/userdata" `
  -Method Post -Body $body -ContentType "application/json"
```

### 3. Verify Service Bus Messages

Go to Azure Portal → Service Bus → Queues → `userdata-queue` → Check "Active Messages"

### 4. Set Up Function App (Optional)

Follow instructions in `FUNCTION-APP-EXAMPLE.md` to create a Function App that processes the Service Bus messages.

## Troubleshooting

### Issue: Build fails

```powershell
# Clear and restore
dotnet clean
dotnet restore
dotnet build
```

### Issue: Can't connect to Service Bus locally

For local testing without Azure Service Bus:

1. Use Azure Service Bus Emulator (if available)
2. Or mock the service for development
3. Or connect to a dev Service Bus namespace in Azure

### Issue: CORS errors

Update `Cors__AllowedOrigins__0` in App Service Configuration to match your static web app URL exactly.

### Issue: 403 Forbidden on Service Bus

Verify Managed Identity has "Azure Service Bus Data Sender" role:

```powershell
az role assignment list --assignee <managed-identity-principal-id>
```

## Next Steps

- ✅ Deploy the application
- ✅ Update static web app with API URL
- ✅ Create Function App (see FUNCTION-APP-EXAMPLE.md)
- ✅ Set up Application Insights monitoring
- ✅ Configure alerts for errors
- ✅ Add authentication/authorization if needed
- ✅ Set up CI/CD pipeline

## Resources

- **README.md** - Complete documentation
- **FUNCTION-APP-EXAMPLE.md** - Function App integration example
- **example-static-web-app.html** - Static web app example
- **deploy.ps1** - Automated deployment script

## Get Help

- Check logs: Azure Portal → App Service → Log stream
- Monitor: Azure Portal → Application Insights
- Service Bus metrics: Azure Portal → Service Bus → Metrics

---

**Need more help?** Check the full [README.md](README.md) for detailed documentation.
