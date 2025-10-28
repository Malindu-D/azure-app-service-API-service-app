using Microsoft.AspNetCore.Mvc;
using UserDataApi.Models;
using UserDataApi.Services;

namespace UserDataApi.Controllers;

/// <summary>
/// API Controller for receiving email notification requests from static web application
/// and forwarding directly to email export app via HTTP
/// Email export app handles database queries and Azure Communication Services
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IEmailExportService _emailExportService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(IEmailExportService emailExportService, ILogger<NotificationController> logger)
    {
        _emailExportService = emailExportService;
        _logger = logger;
    }

    /// <summary>
    /// Receives email notification request from static web application
    /// and forwards it directly to email export app via HTTP
    /// Email export app will fetch data from database and send email via Azure Communication Services
    /// </summary>
    /// <param name="request">Email notification data (recipientEmail, templateId, dataId)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response with correlation ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendNotification(
        [FromBody] EmailNotificationRequest request, 
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        _logger.LogInformation(
            "Received email notification request. CorrelationId: {CorrelationId}, Recipient: {Recipient}, TemplateId: {TemplateId}, DataId: {DataId}", 
            correlationId, request.RecipientEmail, request.TemplateId ?? "N/A", request.DataId ?? "N/A");

        try
        {
            var now = DateTime.UtcNow;

            // Forward directly to email export app via HTTP
            var success = await _emailExportService.SendNotificationAsync(request, cancellationToken);

            if (success)
            {
                return Ok(new 
                { 
                    Success = true,
                    Message = "Email notification sent to email export app successfully",
                    CorrelationId = correlationId,
                    ReceivedAt = now,
                    Status = "Forwarded",
                    Recipient = request.RecipientEmail,
                    TemplateId = request.TemplateId,
                    DataId = request.DataId
                });
            }
            else
            {
                return StatusCode(500, new 
                { 
                    Success = false,
                    Message = "Email export app returned error",
                    CorrelationId = correlationId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forward email notification. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new 
            { 
                Success = false,
                Message = "Failed to forward email notification to email export app. Please try again later.",
                CorrelationId = correlationId
            });
        }
    }

    /// <summary>
    /// Health check endpoint for notification service
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new 
        { 
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "NotificationApi",
            ConnectionType = "Direct HTTP to Email Export App"
        });
    }
}
