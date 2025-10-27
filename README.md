# User Data API - Azure App Service

ASP.NET Core Web API that receives user data (name and age) from a static web application and forwards it to Azure Service Bus for processing by an Azure Function App.

## Architecture

```
Static Web App → Azure App Service (this API) → Azure Service Bus → Function App
```

## Features

- ✅ **REST API** with POST endpoint to receive name and age
- ✅ **Azure Service Bus Integration** for reliable message queuing
- ✅ **Managed Identity Authentication** for secure, passwordless connections
- ✅ **CORS Support** for static web application requests
- ✅ **Retry Logic** with exponential backoff for transient failures
- ✅ **Application Insights** integration for monitoring
- ✅ **Health Check** endpoint
- ✅ **Input Validation** with detailed error messages
- ✅ **Structured Logging** with correlation IDs

## Prerequisites

- .NET 8.0 SDK or later
- Azure CLI
- Azure subscription
- Static Web Application (for sending requests)
- Azure Function App (for consuming messages)

## Local Development

### 1. Clone or navigate to the project

```powershell
cd c:\Users\malin\OneDrive\Desktop\test-api-service-app
```

### 2. Install dependencies

```powershell
dotnet restore
```

### 3. Update configuration

Edit `appsettings.Development.json` with your local/dev Service Bus details:

```json
{
  "ServiceBus": {
    "Namespace": "your-servicebus-namespace.servicebus.windows.net",
    "QueueOrTopicName": "userdata-queue"
  }
}
```

### 4. Run the application

```powershell
dotnet run
```

The API will be available at: `https://localhost:7001` (or check console output)

### 5. Test the API

**Using PowerShell:**

```powershell
$body = @{
    name = "John Doe"
    age = 25
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7001/api/userdata" -Method Post -Body $body -ContentType "application/json"
```

**Using curl:**

```powershell
curl -X POST https://localhost:7001/api/userdata `
  -H "Content-Type: application/json" `
  -d '{\"name\":\"John Doe\",\"age\":25}'
```

**Expected Response:**

```json
{
  "success": true,
  "message": "User data received and queued for processing",
  "correlationId": "12345678-1234-1234-1234-123456789abc",
  "receivedAt": "2025-10-27T10:30:00.123Z"
}
```

## API Endpoints

### POST /api/userdata

Submit user data to be queued for processing.

**Request Body:**

```json
{
  "name": "John Doe",
  "age": 25
}
```

**Validation Rules:**

- `name`: Required, 1-100 characters
- `age`: Required, 1-150

**Response (200 OK):**

```json
{
  "success": true,
  "message": "User data received and queued for processing",
  "correlationId": "unique-guid",
  "receivedAt": "2025-10-27T10:30:00.123Z"
}
```

**Response (400 Bad Request):**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Name is required"],
    "Age": ["Age must be between 1 and 150"]
  }
}
```

**Response (500 Internal Server Error):**

```json
{
  "success": false,
  "message": "Failed to process user data. Please try again later.",
  "correlationId": "unique-guid"
}
```

### GET /api/userdata/health

Health check endpoint.

**Response (200 OK):**

```json
{
  "status": "Healthy",
  "timestamp": "2025-10-27T10:30:00.123Z",
  "service": "UserDataApi"
}
```

### GET /health

Application health check (built-in).

## Azure Deployment

### Option 1: Using PowerShell Deployment Script

1. **Update the script variables** in `deploy.ps1`:

   - Update `$RESOURCE_GROUP`, `$LOCATION`, etc.
   - Update the CORS origin to match your static web app URL

2. **Run the deployment script:**

```powershell
.\deploy.ps1
```

This script will:

- Create Azure Resource Group
- Create Service Bus namespace and queue
- Create App Service Plan and App Service
- Enable Managed Identity
- Grant Service Bus Data Sender role to the Managed Identity
- Configure App Service settings
- Publish and deploy the application

### Option 2: Using Azure Developer CLI (azd)

```powershell
# Login to Azure
azd auth login

# Provision and deploy
azd up
```

### Option 3: Manual Deployment

#### 1. Create Azure Resources

```powershell
# Variables
$RESOURCE_GROUP = "rg-userdata-api"
$LOCATION = "eastus"
$SERVICE_BUS_NAMESPACE = "sb-userdata-unique"
$QUEUE_NAME = "userdata-queue"
$APP_SERVICE_PLAN = "asp-userdata-api"
$APP_SERVICE_NAME = "app-userdata-api-unique"

# Create Resource Group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Service Bus
az servicebus namespace create `
  --resource-group $RESOURCE_GROUP `
  --name $SERVICE_BUS_NAMESPACE `
  --location $LOCATION `
  --sku Standard

az servicebus queue create `
  --resource-group $RESOURCE_GROUP `
  --namespace-name $SERVICE_BUS_NAMESPACE `
  --name $QUEUE_NAME

# Create App Service
az appservice plan create `
  --name $APP_SERVICE_PLAN `
  --resource-group $RESOURCE_GROUP `
  --sku B1 `
  --is-linux

az webapp create `
  --resource-group $RESOURCE_GROUP `
  --plan $APP_SERVICE_PLAN `
  --name $APP_SERVICE_NAME `
  --runtime "DOTNETCORE:8.0"
```

#### 2. Enable Managed Identity

```powershell
az webapp identity assign `
  --name $APP_SERVICE_NAME `
  --resource-group $RESOURCE_GROUP

# Get Principal ID
$PRINCIPAL_ID = az webapp identity show `
  --name $APP_SERVICE_NAME `
  --resource-group $RESOURCE_GROUP `
  --query principalId -o tsv

# Grant Service Bus permissions
$SERVICE_BUS_ID = az servicebus namespace show `
  --resource-group $RESOURCE_GROUP `
  --name $SERVICE_BUS_NAMESPACE `
  --query id -o tsv

az role assignment create `
  --assignee $PRINCIPAL_ID `
  --role "Azure Service Bus Data Sender" `
  --scope $SERVICE_BUS_ID
```

#### 3. Configure App Settings

```powershell
az webapp config appsettings set `
  --name $APP_SERVICE_NAME `
  --resource-group $RESOURCE_GROUP `
  --settings `
    "ServiceBus__Namespace=$SERVICE_BUS_NAMESPACE.servicebus.windows.net" `
    "ServiceBus__QueueOrTopicName=$QUEUE_NAME" `
    "Cors__AllowedOrigins__0=https://your-static-web-app.azurestaticapps.net"
```

#### 4. Deploy Application

```powershell
# Publish
dotnet publish -c Release -o ./publish

# Create ZIP
Compress-Archive -Path ./publish/* -DestinationPath app.zip -Force

# Deploy
az webapp deployment source config-zip `
  --resource-group $RESOURCE_GROUP `
  --name $APP_SERVICE_NAME `
  --src app.zip
```

## Configuration

### App Settings (Environment Variables)

Configure these in Azure Portal → App Service → Configuration or via Azure CLI:

| Setting                                 | Description                | Example                              |
| --------------------------------------- | -------------------------- | ------------------------------------ |
| `ServiceBus__Namespace`                 | Service Bus namespace FQDN | `sb-userdata.servicebus.windows.net` |
| `ServiceBus__QueueOrTopicName`          | Queue or topic name        | `userdata-queue`                     |
| `Cors__AllowedOrigins__0`               | Static web app URL         | `https://app.azurestaticapps.net`    |
| `ApplicationInsights__ConnectionString` | App Insights connection    | (auto-configured)                    |

### CORS Configuration

Update allowed origins in `appsettings.json` or App Service configuration:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://your-static-web-app.azurestaticapps.net",
      "http://localhost:3000"
    ]
  }
}
```

## Service Bus Message Format

Messages sent to Service Bus have the following JSON structure:

```json
{
  "name": "John Doe",
  "age": 25,
  "receivedAt": "2025-10-27T10:30:00.123Z",
  "correlationId": "12345678-1234-1234-1234-123456789abc"
}
```

**Message Properties:**

- `ContentType`: `application/json`
- `CorrelationId`: Unique identifier for tracking
- `MessageId`: Unique GUID
- `ApplicationProperties`:
  - `MessageType`: `UserData`
  - `ReceivedAt`: Timestamp

## Integrating with Static Web App

### JavaScript/TypeScript Example

```javascript
async function submitUserData(name, age) {
  const apiUrl = "https://your-app-service.azurewebsites.net/api/userdata";

  try {
    const response = await fetch(apiUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ name, age }),
    });

    const result = await response.json();

    if (response.ok) {
      console.log("Success:", result);
      return result;
    } else {
      console.error("Error:", result);
      throw new Error(result.message || "Failed to submit data");
    }
  } catch (error) {
    console.error("Request failed:", error);
    throw error;
  }
}

// Usage
submitUserData("John Doe", 25)
  .then((result) => console.log("Correlation ID:", result.correlationId))
  .catch((error) => console.error("Error:", error));
```

### HTML Form Example

```html
<!DOCTYPE html>
<html>
  <head>
    <title>User Data Form</title>
  </head>
  <body>
    <form id="userForm">
      <label>Name: <input type="text" id="name" required /></label><br />
      <label
        >Age: <input type="number" id="age" min="1" max="150" required /></label
      ><br />
      <button type="submit">Submit</button>
    </form>
    <div id="result"></div>

    <script>
      document
        .getElementById("userForm")
        .addEventListener("submit", async (e) => {
          e.preventDefault();

          const name = document.getElementById("name").value;
          const age = parseInt(document.getElementById("age").value);

          try {
            const response = await fetch(
              "https://your-app-service.azurewebsites.net/api/userdata",
              {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name, age }),
              }
            );

            const result = await response.json();
            document.getElementById(
              "result"
            ).innerHTML = `<p style="color: green">Success! Correlation ID: ${result.correlationId}</p>`;
          } catch (error) {
            document.getElementById(
              "result"
            ).innerHTML = `<p style="color: red">Error: ${error.message}</p>`;
          }
        });
    </script>
  </body>
</html>
```

## Function App Integration

The Function App should be configured with a Service Bus trigger to consume messages:

### C# Function Example

```csharp
[Function("ProcessUserData")]
public async Task Run(
    [ServiceBusTrigger("userdata-queue", Connection = "ServiceBusConnection")]
    string messageBody)
{
    var userData = JsonSerializer.Deserialize<UserDataMessage>(messageBody);

    _logger.LogInformation(
        "Processing user data: Name={Name}, Age={Age}, CorrelationId={CorrelationId}",
        userData.Name, userData.Age, userData.CorrelationId);

    // Process the data...
}
```

## Monitoring

### Application Insights

View telemetry in Azure Portal → Application Insights:

- Request traces
- Dependency calls (Service Bus)
- Exceptions
- Custom events

### Logs

View logs in Azure Portal → App Service → Log stream or query via Kusto:

```kusto
traces
| where message contains "UserData"
| order by timestamp desc
| take 100
```

## Security Best Practices

✅ **Managed Identity** - No credentials in code or configuration  
✅ **HTTPS Only** - All traffic encrypted  
✅ **Input Validation** - Data annotations and model validation  
✅ **CORS** - Restricted to specific origins  
✅ **Least Privilege** - Service Bus Data Sender role only  
✅ **Error Handling** - No sensitive data in error responses  
✅ **Logging** - Correlation IDs for request tracking

## Troubleshooting

### Issue: 403 Forbidden when sending to Service Bus

**Solution:** Verify Managed Identity has "Azure Service Bus Data Sender" role:

```powershell
az role assignment list --assignee <principal-id> --scope <service-bus-id>
```

### Issue: CORS errors from static web app

**Solution:** Add your static web app URL to CORS allowed origins:

```powershell
az webapp config appsettings set `
  --name $APP_SERVICE_NAME `
  --resource-group $RESOURCE_GROUP `
  --settings "Cors__AllowedOrigins__0=https://your-static-web-app.azurestaticapps.net"
```

### Issue: Application won't start

**Solution:** Check App Service logs:

```powershell
az webapp log tail --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP
```

## Project Structure

```
test-api-service-app/
├── Controllers/
│   └── UserDataController.cs      # API endpoints
├── Models/
│   ├── UserDataRequest.cs         # Request model
│   └── UserDataMessage.cs         # Service Bus message model
├── Services/
│   ├── IServiceBusService.cs      # Service interface
│   └── ServiceBusService.cs       # Service Bus implementation
├── Program.cs                      # App configuration
├── appsettings.json               # Configuration
├── appsettings.Development.json   # Dev configuration
├── UserDataApi.csproj             # Project file
├── azure.yaml                      # Azure Developer CLI config
├── deploy.ps1                      # PowerShell deployment script
├── deploy.sh                       # Bash deployment script
└── README.md                       # This file
```

## License

MIT

## Support

For issues or questions:

1. Check the troubleshooting section
2. Review Application Insights logs
3. Check Azure Service Bus metrics
4. Review App Service logs
