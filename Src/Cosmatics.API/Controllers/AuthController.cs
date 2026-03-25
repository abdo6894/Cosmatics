
using Cosmatics.Applicatiion.DTOs;
using Cosmatics.Application.DTOs;
using Cosmatics.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cosmatics.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (success, message, otp) = await _authService.RegisterAsync(dto);

        if (!success)
            return BadRequest(new { message });

        // ✅ DEBUG ONLY
        if (HttpContext.Request.Host.Host.Contains("localhost"))
        {
            return Ok(new
            {
                message,
                debugOtp = otp
            });
        }

        return Ok(new { message });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        _logger.LogInformation("Login attempt for User: {Username} / Phone: {Phone}", dto.Username, dto.PhoneNumber);

        var (accessToken, refreshToken, user, error) = await _authService.LoginAsync(dto);

        if (error != null)
        {
            _logger.LogWarning("Login failed for {Username}/{Phone}: {Error}", dto.Username, dto.PhoneNumber, error);

            if (error.Contains("verified"))
                return StatusCode(403, new { message = error });

            return Unauthorized(new { message = error });
        }

        if (accessToken == null || refreshToken == null || user == null)
        {
            _logger.LogWarning("Login failed for {Username}/{Phone}: Invalid credentials", dto.Username, dto.PhoneNumber);
            return Unauthorized(new { message = "Invalid credentials." });
        }

        _logger.LogInformation("Login successful for User: {Username}", user.Username);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,

            Email = user.Email,
            CountryCode = user.CountryCode,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            ProfilePhotoUrl = user.ProfilePhotoUrl
        };

   
        return Ok(new
        {
            Token = accessToken,           
            RefreshToken = refreshToken,  
            User = userDto
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
    {
        var (accessToken, newRefreshToken, error) = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (error != null)
        {
            return Unauthorized(new { message = error });
        }

        return Ok(new
        {
            Token = accessToken,            
            RefreshToken = newRefreshToken  
        });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
    {
        var (isValid, message) = await _authService.VerifyOtpAsync(dto);
        if (!isValid)
        {
            return BadRequest(new { message });
        }
        return Ok(new { message });
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpDto dto)
    {
        var (success, message) = await _authService.ResendOtpAsync(dto);
        if (!success)
        {
            return BadRequest(new { message });
        }
        return Ok(new { message });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var (success, message) = await _authService.ForgotPasswordAsync(dto);
        if (!success)
        {
            return BadRequest(new { message });
        }
        return Ok(new { message });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var (success, message) = await _authService.ResetPasswordAsync(dto);
        if (!success)
        {
            return BadRequest(new { message });
        }
        return Ok(new { message });
    }

    [HttpPut("update-profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var (success,message) = await _authService.UpdateProfileAsync(userId.Value, dto);
        if (!success)
            return BadRequest(new { message = "Failed to update profile." });

        return Ok(new { message = "Profile updated successfully." });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await _authService.LogoutAsync(userId.Value);
        }
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId.Value);
        if (user == null) return NotFound();

        return Ok(new
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            CountryCode = user.CountryCode,
            ProfilePhotoUrl = user.ProfilePhotoUrl
        });
    }

    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deletedUser = await _authService.DeleteUserAsync(id);
        if (deletedUser == null)
            return NotFound(new { message = "User not found." });

        return Ok(new { message = "User deleted successfully.", user = deletedUser });
    }

    [HttpDelete("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllUsers()
    {
        await _authService.DeleteAllUsersAsync();
        return Ok(new { message = "All users deleted successfully." });
    }

    private int? GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId)
            ? userId
            : null;
    }
}