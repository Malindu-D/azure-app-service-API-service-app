using Microsoft.AspNetCore.Mvc;
using UserDataApi.Models;
using UserDataApi.Services;

namespace UserDataApi.Controllers;

/// <summary>
/// API Controller for receiving user data from static web application
/// and forwarding to Azure Service Bus for Function App processing
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserDataController : ControllerBase
{
    private readonly IServiceBusService _serviceBusService;
    private readonly ILogger<UserDataController> _logger;

    public UserDataController(IServiceBusService serviceBusService, ILogger<UserDataController> logger)
    {
        _serviceBusService = serviceBusService;
        _logger = logger;
    }

    /// <summary>
    /// Receives user data (name and age) from static web application
    /// and sends it to Service Bus queue/topic for Function App processing
    /// </summary>
    /// <param name="request">User data containing name and age</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response with correlation ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitUserData(
        [FromBody] UserDataRequest request, 
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        _logger.LogInformation("Received user data submission. CorrelationId: {CorrelationId}, Name: {Name}, Age: {Age}", 
            correlationId, request.Name, request.Age);

        try
        {
            // Create message for Service Bus
            var message = new UserDataMessage
            {
                Name = request.Name,
                Age = request.Age,
                ReceivedAt = DateTime.UtcNow,
                CorrelationId = correlationId
            };

            // Send to Service Bus
            await _serviceBusService.SendUserDataAsync(message, cancellationToken);

            return Ok(new 
            { 
                Success = true,
                Message = "User data received and queued for processing",
                CorrelationId = correlationId,
                ReceivedAt = message.ReceivedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process user data. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new 
            { 
                Success = false,
                Message = "Failed to process user data. Please try again later.",
                CorrelationId = correlationId
            });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new 
        { 
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "UserDataApi"
        });
    }
}
