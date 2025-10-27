using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using UserDataApi.Models;

namespace UserDataApi.Services;

/// <summary>
/// Service Bus implementation using Managed Identity for secure authentication
/// Implements retry logic with exponential backoff for transient failures
/// </summary>
public class ServiceBusService : IServiceBusService, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusService> _logger;
    private readonly string _queueOrTopicName;

    public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
    {
        _logger = logger;
        
        var serviceBusNamespace = configuration["ServiceBus:Namespace"] 
            ?? throw new InvalidOperationException("ServiceBus:Namespace configuration is missing");
        
        _queueOrTopicName = configuration["ServiceBus:QueueOrTopicName"] 
            ?? throw new InvalidOperationException("ServiceBus:QueueOrTopicName configuration is missing");

        // Use Managed Identity for authentication (best practice for Azure-hosted apps)
        var credential = new DefaultAzureCredential();
        
        var clientOptions = new ServiceBusClientOptions
        {
            RetryOptions = new ServiceBusRetryOptions
            {
                Mode = ServiceBusRetryMode.Exponential,
                MaxRetries = 3,
                Delay = TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromSeconds(30)
            }
        };

        _client = new ServiceBusClient(serviceBusNamespace, credential, clientOptions);
        _sender = _client.CreateSender(_queueOrTopicName);

        _logger.LogInformation("ServiceBusService initialized for namespace: {Namespace}, queue/topic: {QueueOrTopic}", 
            serviceBusNamespace, _queueOrTopicName);
    }

    public async Task SendUserDataAsync(UserDataMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Serialize message to JSON
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                CorrelationId = message.CorrelationId,
                MessageId = Guid.NewGuid().ToString()
            };

            // Add custom properties for filtering/routing
            serviceBusMessage.ApplicationProperties.Add("MessageType", "UserData");
            serviceBusMessage.ApplicationProperties.Add("ReceivedAt", message.ReceivedAt);

            // Send message with retry logic
            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation("Successfully sent message to Service Bus. CorrelationId: {CorrelationId}, Name: {Name}", 
                message.CorrelationId, message.Name);
        }
        catch (ServiceBusException ex) when (ex.IsTransient)
        {
            // Transient errors - already retried by SDK
            _logger.LogError(ex, "Transient Service Bus error for CorrelationId: {CorrelationId}", message.CorrelationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to Service Bus. CorrelationId: {CorrelationId}", message.CorrelationId);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
        _logger.LogInformation("ServiceBusService disposed");
    }
}
