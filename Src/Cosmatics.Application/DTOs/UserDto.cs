using Cosmatics.Domain.Enums;

namespace Cosmatics.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public string? ProfilePhotoUrl { get; set; }
}
