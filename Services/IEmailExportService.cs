using UserDataApi.Models;

namespace UserDataApi.Services;

/// <summary>
/// Interface for calling email export application directly via HTTP
/// Email export app is responsible for fetching data from database
/// and sending emails via Azure Communication Services
/// </summary>
public interface IEmailExportService
{
    /// <summary>
    /// Forwards email notification request directly to email export app
    /// </summary>
    /// <param name="request">Email notification request with recipient and IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> SendNotificationAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default);
}
