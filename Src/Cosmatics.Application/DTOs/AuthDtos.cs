using Cosmatics.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;

public class RegisterDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    
    [Required]
    [RegularExpression(@"^\+\d+$", ErrorMessage = "Country Code must start with + and match any country code format.")]
    public string CountryCode { get; set; } = string.Empty;
    
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.Customer;
}
