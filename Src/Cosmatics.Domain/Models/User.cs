using Cosmatics.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty; // Optional now
    [Required]
    public string CountryCode { get; set; } = string.Empty;
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsVerified { get; set; } = false;
    public string? ProfilePhotoUrl { get; set; }
    public string? ActiveToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiration { get; set; }
}
