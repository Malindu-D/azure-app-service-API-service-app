namespace UserDataApi.Models;

/// <summary>
/// Message model for Service Bus queue/topic
/// </summary>
public class UserDataMessage
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}
