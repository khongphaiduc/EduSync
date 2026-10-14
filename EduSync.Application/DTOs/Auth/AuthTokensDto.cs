using EduSync.Domain.Enums;

namespace EduSync.Application.DTOs.Auth;

public sealed record AuthTokensDto(
    Guid UserId,
    string Email,
    UserRole Role,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
