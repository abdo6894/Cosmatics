using Cosmatics.Application;
using Cosmatics.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Cosmatics.Infrastructure.Persistense.Data;
using Microsoft.Extensions.Configuration;
using Cosmatics.Application.DTOs;
using Cosmatics.Application.Common;

namespace Cosmatics.Infrastructure.Services;

public class AuthService : IAuthService
{
    private ICacheService _cacheService;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<CountryCode> _countryRepo;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(IRepository<User> userRepo, IRepository<CountryCode> countryRepo, IConfiguration configuration, IEmailService emailService,ICacheService cacheService)
    {
        _userRepo = userRepo;
        _countryRepo = countryRepo;
        _configuration = configuration;
        _emailService = emailService;
        _cacheService = cacheService;
    }

    public async Task<(bool Success, string Message, string? otp)> RegisterAsync(RegisterDto dto)
    {
        string? otp = null; // ⭐ مهم جدًا

        try
        {
            Console.WriteLine($"[Register] Registering user: {dto.Username}");

            var validCountry = await _countryRepo.FindAsync(c => c.Code == dto.CountryCode);
            var country = validCountry.FirstOrDefault();
            if (country == null)
                return (false, "Invalid Country Code.", null);

            var existingUsers = await _userRepo.FindAsync(u =>
                (u.PhoneNumber == dto.PhoneNumber && u.CountryCode == dto.CountryCode) ||
                u.Email == dto.Email ||
                u.Username == dto.Username
            );

            if (existingUsers.Any(u => u.PhoneNumber == dto.PhoneNumber && u.CountryCode == dto.CountryCode))
                return (false, "Phone Number already exists.", null);

            if (existingUsers.Any(u => u.Email == dto.Email))
                return (false, "Email already exists.", null);

            if (existingUsers.Any(u => u.Username == dto.Username))
                return (false, "Username already exists.", null);

            CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            otp = GenerateOtp();

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email ?? "",
                CountryCode = dto.CountryCode,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = Convert.ToBase64String(passwordHash) + ":" + Convert.ToBase64String(passwordSalt),
                Role = dto.Role,
                IsVerified = false,
                OtpCode = otp,
                OtpExpiration = DateTime.UtcNow.AddMinutes(10)
            };

            await _userRepo.AddAsync(user);

            if (!string.IsNullOrEmpty(user.Email) && !HttpContextIsLocal())
                await _emailService.SendEmailAsync(user.Email, "Cosmatics Verification Code", $"Your verification code is: {otp}");

            return (true, "User registered successfully.", otp);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Register] Error: {ex}");
            return (false, "Internal Server Error.", otp);
        }
    }
    private bool HttpContextIsLocal()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return env == "Development";
    }




    public async Task<(string? accessToken, string? refreshToken, User? user, string? error)> LoginAsync(LoginDto dto)
    {
        try
        {
            var validCountry = await _countryRepo.FindAsync(c => c.Code == dto.CountryCode);
            if (!validCountry.Any())
                return (null, null, null, "Invalid Country Code.");

            IEnumerable<User> users;

            if (!string.IsNullOrEmpty(dto.Username))
            {
                users = await _userRepo.FindAsync(u => u.Username == dto.Username);
            }
            else
            {
                users = await _userRepo.FindAsync(u => u.PhoneNumber == dto.PhoneNumber && u.CountryCode == dto.CountryCode);
            }

            var user = users.FirstOrDefault();

            if (user == null)
                return (null, null, null, "Invalid credentials.");

            if (!user.IsVerified)
                return (null, null, null, "Account not verified. Please verify your phone number first.");

            if (!VerifyPasswordHash(dto.Password, user.PasswordHash))
                return (null, null, null, "Invalid credentials.");

            var accessToken = CreateToken(user);
            var refreshToken = GenerateRefreshToken();

            user.ActiveToken = accessToken;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7));

            await _userRepo.UpdateAsync(user);
            await _cacheService.RemoveDataAsync($"user:{user.Id}");

            Console.WriteLine($"[Login] User {user.Username} logged in successfully");

            return (accessToken, refreshToken, user, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Login] Error: {ex.Message}");
            return (null, null, null, "An error occurred during login.");
        }
    }

    public async Task<(string? accessToken, string? refreshToken, string? error)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var users = await _userRepo.FindAsync(u => u.RefreshToken == refreshToken);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                Console.WriteLine("[RefreshToken] Invalid refresh token");
                return (null, null, "Invalid refresh token.");
            }

            if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                Console.WriteLine($"[RefreshToken] Refresh token expired for user {user.Id}");
                return (null, null, "Refresh token expired. Please login again.");
            }

            var newAccessToken = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.ActiveToken = newAccessToken;
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7));

            await _userRepo.UpdateAsync(user);
            await _cacheService.RemoveDataAsync($"user:{user.Id}");

            Console.WriteLine($"[RefreshToken] Tokens refreshed for user {user.Id}");

            return (newAccessToken, newRefreshToken, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RefreshToken] Error: {ex.Message}");
            return (null, null, "An error occurred during token refresh.");
        }
    }

    public async Task RevokeRefreshTokenAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepo.UpdateAsync(user);
    
            Console.WriteLine($"[RevokeRefreshToken] Refresh token revoked for user {userId}");
        }
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var validCountry = await _countryRepo.FindAsync(c => c.Code == dto.CountryCode);
        if (!validCountry.Any())
            return (false, "Invalid Country Code.");

        var users = await _userRepo.FindAsync(u => u.PhoneNumber == dto.PhoneNumber && u.CountryCode == dto.CountryCode);
        if (!users.Any())
            return (false, "User not found.");

        var user = users.First();
        var otp = GenerateOtp();
        user.OtpCode = otp;
        user.OtpExpiration = DateTime.UtcNow.AddMinutes(10);

        await _userRepo.UpdateAsync(user);

        if (!string.IsNullOrEmpty(user.Email))
        {
            try
            {
                await _emailService.SendEmailAsync(user.Email, "Cosmatics Password Reset Code", $"Your password reset code is: {otp}");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send OTP: {ex.Message}");
            }
        }

        return (true, "OTP sent successfully to your email.");
    }

    public async Task<(bool Success, string Message)> ResendOtpAsync(ResendOtpDto dto)
    {
        var validCountry = await _countryRepo.FindAsync(c => c.Code == dto.CountryCode);
        if (!validCountry.Any())
            return (false, "Invalid Country Code.");

        var users = await _userRepo.FindAsync(u => u.PhoneNumber == dto.PhoneNumber && u.CountryCode == dto.CountryCode);
        if (!users.Any())
            return (false, "User not found.");

        var user = users.First();

        var otp = GenerateOtp();
        user.OtpCode = otp;
        user.OtpExpiration = DateTime.UtcNow.AddMinutes(10);

        await _userRepo.UpdateAsync(user);

        if (!string.IsNullOrEmpty(user.Email))
        {
            try
            {
                await _emailService.SendEmailAsync(user.Email, "Cosmatics Verification Code", $"Your new verification code is: {otp}");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send OTP: {ex.Message}");
            }
        }

        return (true, "OTP resent successfully.");
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var validCountry = await _countryRepo.FindAsync(c => c.Code == dto.CountryCode);
        if (!validCountry.Any())
            return (false, "Invalid Country Code.");

        if (dto.NewPassword != dto.ConfirmPassword)
            return (false, "Passwords do not match.");

        var users = await _userRepo.FindAsync(u => u.PhoneNumber == dto.PhoneNumber && u.CountryCode == dto.CountryCode);
        var user = users.FirstOrDefault();
        if (user == null)
            return (false, "User not found.");

        CreatePasswordHash(dto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = Convert.ToBase64String(passwordHash) + ":" + Convert.ToBase64String(passwordSalt);

        await _userRepo.UpdateAsync(user);
        return (true, "Password reset successfully.");
    }

    public async Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var validCountry = await _countryRepo.FindAsync(c => c.Code == dto.CountryCode);
        if (!validCountry.Any())
            return (false, "Invalid Country Code.");

        var users = await _userRepo.FindAsync(u => u.PhoneNumber == dto.PhoneNumber && u.CountryCode == dto.CountryCode);
        var user = users.FirstOrDefault();

        if (user == null)
            return (false, "User not found.");

        if (user.OtpCode == dto.OtpCode && user.OtpExpiration > DateTime.UtcNow)
        {
            user.IsVerified = true;
            user.OtpCode = null;
            user.OtpExpiration = null;
            await _userRepo.UpdateAsync(user);
            await _cacheService.RemoveDataAsync($"user:{user.Id}");
            return (true, "OTP verified successfully.");
        }

        return (false, "Invalid or expired OTP code.");
    }
    public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found.");

        if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
        {
            var existingUsername = await _userRepo.FindAsync(u => u.Username == dto.Username && u.Id != userId);
            if (existingUsername.Any())
                return (false, "Username already exists.");

            user.Username = dto.Username;
        }

        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            var existingEmail = await _userRepo.FindAsync(u => u.Email == dto.Email && u.Id != userId);
            if (existingEmail.Any())
                return (false, "Email already exists.");

            user.Email = dto.Email;
        }

        if (!string.IsNullOrEmpty(dto.ProfilePhotoUrl))
            user.ProfilePhotoUrl = dto.ProfilePhotoUrl;

        await _userRepo.UpdateAsync(user);
        await _cacheService.RemoveDataAsync($"user:{user.Id}");
        return (true, "Profile updated successfully.");
    }
   
    public async Task<User?> GetUserByIdAsync(int id)
    {
        var key= $"user:{id}";
        var cachedUser = await _cacheService.GetDataAsync<User>(key);
        if (cachedUser is not null)
        {
            Console.WriteLine("Cache visited");
              return cachedUser;


        }
        var user = await _userRepo.GetByIdAsync(id);
        await _cacheService.SetDataAsync(key, user, TimeSpan.FromMinutes(30));
        return user;
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null)
        {

            user.ActiveToken = null;
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepo.UpdateAsync(user);
            await _cacheService.RemoveDataAsync($"user:{user.Id}");
        }
    }

    //
    public async Task<User?> DeleteUserAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null)
            return null;

        await _userRepo.DeleteAsync(user);
        await _cacheService.RemoveDataAsync($"user:{user.Id}");

        return user;
    }

    public async Task DeleteAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync();
        foreach (var user in users)
        {
            await _userRepo.DeleteAsync(user);
            await _cacheService.RemoveDataAsync($"user:{user.Id}");
        }
    }

    // ========== PRIVATE HELPER METHODS ==========

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPasswordHash(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
            return false;

        var passwordHash = Convert.FromBase64String(parts[0]);
        var passwordSalt = Convert.FromBase64String(parts[1]);

        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // ✅ Use configurable expiration
        var expirationMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 60);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),  // ✅ Use UtcNow
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateOtp()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);
            return (value % 9000 + 1000).ToString();
        }
    }

}
