namespace UserDataApi.Models;

/// <summary>
/// Message model for Service Bus notification queue
/// This will be forwarded to the next application for processing
/// </summary>
public class EmailNotificationMessage
{
    /// <summary>
    /// Output variable name: "recipientEmail"
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>
    /// Output variable name: "subject"
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Output variable name: "message"
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Output variable name: "senderName"
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// Output variable name: "priority"
    /// Values: "Low", "Normal", "High"
    /// </summary>
    public string Priority { get; set; } = "Normal";

    /// <summary>
    /// Output variable name: "metadata"
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Output variable name: "receivedAt"
    /// ISO 8601 format timestamp
    /// </summary>
    public DateTime ReceivedAt { get; set; }

    /// <summary>
    /// Output variable name: "correlationId"
    /// Unique tracking ID for this notification
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Output variable name: "status"
    /// Current status: "Pending", "Queued", "Processing", "Sent", "Failed"
    /// </summary>
    public string Status { get; set; } = "Queued";

    /// <summary>
    /// Output variable name: "queuedAt"
    /// Timestamp when message was queued
    /// </summary>
    public DateTime QueuedAt { get; set; }
}
