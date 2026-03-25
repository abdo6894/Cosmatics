using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [RegularExpression(@"^\+\d+$", ErrorMessage = "Country Code must start with + and match any country code format.")]
    public string CountryCode { get; set; } = string.Empty;
    
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
}
