using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoveLens.Application.Common.Interfaces;
using MoveLens.Application.Features.Identity;
using MoveLens.Application.Features.Identity.Dtos;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MoveLens.Infrastructure.Identity;

public sealed class TokenProvider(
    IConfiguration configuration,
    IAppDbContext context)
    : ITokenProvider
{
    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(
        AppUserDto user,
        CancellationToken ct = default)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var secret = jwtSettings["Secret"]!;
        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(jwtSettings["TokenExpirationInMinutes"]!));


        var claims = new List<Claim>
        {
           new(JwtRegisteredClaimNames.Sub,   user.UserId),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        };

        foreach (var role in user.Roles) claims.Add(new Claim(ClaimTypes.Role, role));
        foreach (var claim in user.Claims) claims.Add(claim);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256Signature),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(descriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        // ── Refresh Token ─────────────────────────────────────────────────
        await context.RefreshTokens
            .Where(rt => rt.UserId == user.UserId)
            .ExecuteDeleteAsync(ct);

        var refreshTokenResult = RefreshToken.Create(
            Guid.NewGuid(),
            GenerateRefreshToken(),
            user.UserId,
            DateTimeOffset.UtcNow.AddDays(7));

        if (!refreshTokenResult.IsSuccess)
            return Error.Unexpected(
                "Token.GenerationFailed",
                "An error occurred while generating the token.");

        context.RefreshTokens.Add(refreshTokenResult.Value);
        await context.SaveChangesAsync(ct);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenResult.Value.Token,
            ExpiresOnUtc = expires,
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string expiredAccessToken)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(expiredAccessToken, parameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase))
                return null;

            return principal;
        }
        catch { return null; }
    }

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}