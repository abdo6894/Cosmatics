using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;


    public class LoginDto
    {
        public string? Username { get; set; } // Added for Dashboard/Admin login

        [Required(ErrorMessage = "Country Code is required.")]
        [RegularExpression(@"^\+\d+$", ErrorMessage = "Country Code must start with + and match any country code format.")]
        public string? CountryCode { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        public string? PhoneNumber { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = string.Empty;
    }
