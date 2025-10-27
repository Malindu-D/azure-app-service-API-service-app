using UserDataApi.Models;

namespace UserDataApi.Services;

/// <summary>
/// Service interface for Azure Service Bus operations
/// </summary>
public interface IServiceBusService
{
    /// <summary>
    /// Send user data message to Service Bus queue/topic
    /// </summary>
    Task SendUserDataAsync(UserDataMessage message, CancellationToken cancellationToken = default);
}
