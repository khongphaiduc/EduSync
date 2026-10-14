using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EduSync.Application.Abstractions.Authentication;
using EduSync.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace EduSync.Infrastructure.Authentication;

public sealed class JwtTokenService(JwtOptions options) : IAuthTokenService
{
    public IssuedTokens Issue(UserEntity user)
    {
        var nowUtc = DateTime.UtcNow;
        var accessExpiresAtUtc = nowUtc.AddMinutes(options.AccessTokenMinutes);
        var refreshExpiresAtUtc = nowUtc.AddDays(options.RefreshTokenDays);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.Name.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: accessExpiresAtUtc,
            signingCredentials: credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

        return new IssuedTokens(accessToken, accessExpiresAtUtc, refreshToken, refreshExpiresAtUtc);
    }

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken))).ToLowerInvariant();
}
