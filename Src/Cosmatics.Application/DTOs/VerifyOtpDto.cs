using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;

public class VerifyOtpDto
{
    [Required]
    [RegularExpression(@"^\+\d+$", ErrorMessage = "Country Code must start with + and match any country code format.")]
    public string CountryCode { get; set; } = string.Empty;
    
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "OTP must be exactly 4 numeric digits.")]
    public string OtpCode { get; set; } = string.Empty;
}
