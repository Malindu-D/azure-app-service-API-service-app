using System.ComponentModel.DataAnnotations;

namespace UserDataApi.Models;

/// <summary>
/// Simplified request model for email notification that will be forwarded to email export app
/// The email export app will fetch data from database and send via Azure Communication Services
/// 
/// INPUT VARIABLES (from static web app):
/// - recipientEmail: Email address of the recipient
/// - templateId: Template identifier for email export app (optional)
/// - dataId: Database record ID for email export app to fetch data (optional)
/// </summary>
public class EmailNotificationRequest
{
    /// <summary>
    /// Recipient's email address - passed to email export app
    /// Variable name: recipientEmail
    /// </summary>
    [Required(ErrorMessage = "Recipient email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>
    /// Template ID for email export app (optional)
    /// Email export app uses this to determine which email template to use
    /// Variable name: templateId
    /// </summary>
    [StringLength(50, ErrorMessage = "Template ID cannot exceed 50 characters")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// Database record ID for email export app to fetch data (optional)
    /// Email export app uses this to query database for email content
    /// Variable name: dataId
    /// </summary>
    [StringLength(100, ErrorMessage = "Data ID cannot exceed 100 characters")]
    public string? DataId { get; set; }
}
