using Cosmatics.Models;
using Cosmatics.Application.DTOs;

namespace Cosmatics.Infrastructure.Services;

public interface IAuthService
{
    Task<(bool Success, string Message,string? otp )> RegisterAsync(RegisterDto dto);
    Task<(string? accessToken, string? refreshToken, User? user, string? error)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpDto dto);
    Task<(bool Success, string Message)> ResendOtpAsync(ResendOtpDto dto);
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
    Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task<User?> GetUserByIdAsync(int id);
    Task LogoutAsync(int userId);
    Task<User?> DeleteUserAsync(int id);
    Task DeleteAllUsersAsync();
    Task<(string? accessToken, string? refreshToken, string? error)> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(int userId);
}
