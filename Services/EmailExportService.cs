using System.Text;
using System.Text.Json;
using UserDataApi.Models;

namespace UserDataApi.Services;

/// <summary>
/// HTTP client service for calling email export application directly
/// Email export app handles database queries and Azure Communication Services integration
/// </summary>
public class EmailExportService : IEmailExportService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailExportService> _logger;
    private readonly string _emailExportAppUrl;

    public EmailExportService(HttpClient httpClient, IConfiguration configuration, ILogger<EmailExportService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _emailExportAppUrl = configuration["EmailExportApp:Url"] 
            ?? throw new InvalidOperationException("EmailExportApp:Url configuration is missing");

        _httpClient.BaseAddress = new Uri(_emailExportAppUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        _logger.LogInformation("EmailExportService initialized for URL: {Url}", _emailExportAppUrl);
    }

    public async Task<bool> SendNotificationAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Prepare payload for email export app
            var payload = new
            {
                recipientEmail = request.RecipientEmail,
                templateId = request.TemplateId,
                dataId = request.DataId
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation(
                "Sending notification to email export app. Recipient: {Recipient}, TemplateId: {TemplateId}, DataId: {DataId}",
                request.RecipientEmail, request.TemplateId ?? "N/A", request.DataId ?? "N/A");

            // Call email export app endpoint
            var response = await _httpClient.PostAsync("/api/send-email", httpContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Email export app accepted request. Recipient: {Recipient}, StatusCode: {StatusCode}",
                    request.RecipientEmail, response.StatusCode);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Email export app returned error. Recipient: {Recipient}, StatusCode: {StatusCode}, Error: {Error}",
                    request.RecipientEmail, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, 
                "HTTP error calling email export app. Recipient: {Recipient}, Url: {Url}",
                request.RecipientEmail, _emailExportAppUrl);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, 
                "Timeout calling email export app. Recipient: {Recipient}, Url: {Url}",
                request.RecipientEmail, _emailExportAppUrl);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to call email export app. Recipient: {Recipient}",
                request.RecipientEmail);
            throw;
        }
    }
}
