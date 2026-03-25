using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;

public class ResetPasswordDto
{
    [Required]
    [RegularExpression(@"^\+\d+$", ErrorMessage = "Country Code must start with + and match any country code format.")]
    public string CountryCode { get; set; } = string.Empty;
    
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8, ErrorMessage = "New Password must be at least 8 characters long.")]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
