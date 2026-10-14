namespace EduSync.Application.Abstractions.Authentication;

public sealed record IssuedTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
