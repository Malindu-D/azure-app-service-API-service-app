using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using UserDataApi.Models;

namespace UserDataApi.Services;

/// <summary>
/// Service Bus implementation using Managed Identity for secure authentication
/// Implements retry logic with exponential backoff for transient failures
/// Supports multiple queues for different message types
/// </summary>
public class ServiceBusService : IServiceBusService, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _userDataSender;
    private readonly ServiceBusSender _notificationSender;
    private readonly ILogger<ServiceBusService> _logger;
    private readonly string _userDataQueueName;
    private readonly string _notificationQueueName;

    public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
    {
        _logger = logger;
        
        var serviceBusNamespace = configuration["ServiceBus:Namespace"] 
            ?? throw new InvalidOperationException("ServiceBus:Namespace configuration is missing");
        
        _userDataQueueName = configuration["ServiceBus:UserDataQueue"] 
            ?? configuration["ServiceBus:QueueOrTopicName"] 
            ?? throw new InvalidOperationException("ServiceBus:UserDataQueue configuration is missing");

        _notificationQueueName = configuration["ServiceBus:NotificationQueue"] 
            ?? "notification-queue";

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
        _userDataSender = _client.CreateSender(_userDataQueueName);
        _notificationSender = _client.CreateSender(_notificationQueueName);

        _logger.LogInformation(
            "ServiceBusService initialized for namespace: {Namespace}, userDataQueue: {UserDataQueue}, notificationQueue: {NotificationQueue}", 
            serviceBusNamespace, _userDataQueueName, _notificationQueueName);
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
            await _userDataSender.SendMessageAsync(serviceBusMessage, cancellationToken);

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

    public async Task SendEmailNotificationAsync(EmailNotificationMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Serialize message to JSON
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                CorrelationId = message.CorrelationId,
                MessageId = Guid.NewGuid().ToString(),
                Subject = message.Subject // Set Service Bus message subject for routing
            };

            // Add custom properties for filtering/routing in the next application
            serviceBusMessage.ApplicationProperties.Add("MessageType", "EmailNotification");
            serviceBusMessage.ApplicationProperties.Add("Priority", message.Priority);
            serviceBusMessage.ApplicationProperties.Add("ReceivedAt", message.ReceivedAt);
            serviceBusMessage.ApplicationProperties.Add("RecipientEmail", message.RecipientEmail);
            serviceBusMessage.ApplicationProperties.Add("Status", message.Status);

            // Send message with retry logic
            await _notificationSender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "Successfully sent notification to Service Bus. CorrelationId: {CorrelationId}, Recipient: {Recipient}, Priority: {Priority}", 
                message.CorrelationId, message.RecipientEmail, message.Priority);
        }
        catch (ServiceBusException ex) when (ex.IsTransient)
        {
            // Transient errors - already retried by SDK
            _logger.LogError(ex, "Transient Service Bus error for notification CorrelationId: {CorrelationId}", message.CorrelationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to Service Bus. CorrelationId: {CorrelationId}", message.CorrelationId);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _userDataSender.DisposeAsync();
        await _notificationSender.DisposeAsync();
        await _client.DisposeAsync();
        _logger.LogInformation("ServiceBusService disposed");
    }
}
