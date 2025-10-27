# Sample Azure Function App - Service Bus Consumer

This is an example Azure Function that consumes messages from the Service Bus queue populated by the API.

## C# Function (Isolated Worker Model)

### Function.cs

```csharp
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace UserDataProcessor;

public class UserDataMessage
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class ProcessUserDataFunction
{
    private readonly ILogger<ProcessUserDataFunction> _logger;

    public ProcessUserDataFunction(ILogger<ProcessUserDataFunction> logger)
    {
        _logger = logger;
    }

    [Function("ProcessUserData")]
    public async Task Run(
        [ServiceBusTrigger("userdata-queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            _logger.LogInformation("Processing message: {MessageId}", message.MessageId);

            // Deserialize message
            var userData = JsonSerializer.Deserialize<UserDataMessage>(message.Body.ToString());

            if (userData == null)
            {
                _logger.LogWarning("Failed to deserialize message: {MessageId}", message.MessageId);
                await messageActions.DeadLetterMessageAsync(message,
                    "DeserializationFailed",
                    "Unable to deserialize message body");
                return;
            }

            _logger.LogInformation(
                "Processing user data - Name: {Name}, Age: {Age}, CorrelationId: {CorrelationId}",
                userData.Name, userData.Age, userData.CorrelationId);

            // TODO: Add your business logic here
            // Examples:
            // - Save to database
            // - Send notification
            // - Call another API
            // - Generate report
            await ProcessUserDataAsync(userData);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);

            _logger.LogInformation(
                "Successfully processed message: {MessageId}, CorrelationId: {CorrelationId}",
                message.MessageId, userData.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing message: {MessageId}. Error: {Error}",
                message.MessageId, ex.Message);

            // Optionally dead letter the message after multiple retries
            if (message.DeliveryCount > 3)
            {
                await messageActions.DeadLetterMessageAsync(message,
                    "ProcessingFailed",
                    $"Failed after {message.DeliveryCount} attempts: {ex.Message}");
            }
            else
            {
                // Let the message retry
                await messageActions.AbandonMessageAsync(message);
            }
        }
    }

    private async Task ProcessUserDataAsync(UserDataMessage userData)
    {
        // Simulate processing
        await Task.Delay(100);

        // Add your business logic here
        _logger.LogInformation("Processing logic for {Name}", userData.Name);

        // Example: Categorize by age group
        var ageGroup = userData.Age switch
        {
            < 18 => "Minor",
            >= 18 and < 65 => "Adult",
            _ => "Senior"
        };

        _logger.LogInformation("User {Name} is in age group: {AgeGroup}", userData.Name, ageGroup);
    }
}
```

### host.json

```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    }
  },
  "extensions": {
    "serviceBus": {
      "prefetchCount": 100,
      "messageHandlerOptions": {
        "autoComplete": false,
        "maxConcurrentCalls": 32,
        "maxAutoRenewDuration": "00:05:00"
      },
      "sessionHandlerOptions": {
        "autoComplete": false,
        "messageWaitTimeout": "00:00:30",
        "maxAutoRenewDuration": "00:05:00",
        "maxConcurrentSessions": 16
      }
    }
  }
}
```

### local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "Endpoint=sb://your-servicebus-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_KEY"
  }
}
```

### UserDataProcessor.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="1.21.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="1.17.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.ServiceBus" Version="5.16.0" />
    <PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.22.0" />
  </ItemGroup>
</Project>
```

### Program.cs

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
```

## Deployment

### 1. Create Function App

```powershell
# Variables
$FUNCTION_APP_NAME = "func-userdata-processor-$(Get-Random)"
$STORAGE_ACCOUNT = "stuserdata$(Get-Random)"

# Create storage account
az storage account create `
  --name $STORAGE_ACCOUNT `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION `
  --sku Standard_LRS

# Create Function App
az functionapp create `
  --resource-group $RESOURCE_GROUP `
  --consumption-plan-location $LOCATION `
  --runtime dotnet-isolated `
  --runtime-version 8 `
  --functions-version 4 `
  --name $FUNCTION_APP_NAME `
  --storage-account $STORAGE_ACCOUNT

# Enable Managed Identity
az functionapp identity assign `
  --name $FUNCTION_APP_NAME `
  --resource-group $RESOURCE_GROUP

# Get Principal ID
$FUNCTION_PRINCIPAL_ID = az functionapp identity show `
  --name $FUNCTION_APP_NAME `
  --resource-group $RESOURCE_GROUP `
  --query principalId -o tsv

# Grant Service Bus Data Receiver role
az role assignment create `
  --assignee $FUNCTION_PRINCIPAL_ID `
  --role "Azure Service Bus Data Receiver" `
  --scope $SERVICE_BUS_ID
```

### 2. Configure Connection String (Using Managed Identity)

```powershell
az functionapp config appsettings set `
  --name $FUNCTION_APP_NAME `
  --resource-group $RESOURCE_GROUP `
  --settings `
    "ServiceBusConnection__fullyQualifiedNamespace=$SERVICE_BUS_NAMESPACE.servicebus.windows.net"
```

### 3. Deploy Function

```powershell
# Publish
func azure functionapp publish $FUNCTION_APP_NAME
```

## Testing

Monitor function executions:

```powershell
# Stream logs
func azure functionapp logstream $FUNCTION_APP_NAME

# Or via Azure CLI
az webapp log tail --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP
```

## Message Flow

1. **Static Web App** → Sends POST request with name and age
2. **App Service API** → Receives request, validates, sends to Service Bus
3. **Service Bus Queue** → Stores message reliably
4. **Function App** → Triggered automatically, processes message
5. **Business Logic** → Your custom processing (database, notifications, etc.)

## Next Steps

- Add database integration (Azure SQL, Cosmos DB)
- Implement notification service (SendGrid, Azure Communication Services)
- Add dead letter queue monitoring
- Set up alerting for failed messages
- Implement retry policies
- Add message batching for high throughput
