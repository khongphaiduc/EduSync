using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Auth;

public sealed class RefreshTokenRequest
{
    [Required, StringLength(512, MinimumLength = 20)]
    public string RefreshToken { get; init; } = string.Empty;
}
