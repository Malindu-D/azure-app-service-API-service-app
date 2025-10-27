using System.ComponentModel.DataAnnotations;

namespace UserDataApi.Models;

/// <summary>
/// Request model for user data received from static web application
/// </summary>
public class UserDataRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Age is required")]
    [Range(1, 150, ErrorMessage = "Age must be between 1 and 150")]
    public int Age { get; set; }
}
