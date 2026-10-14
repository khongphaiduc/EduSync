using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Auth;

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; init; } = string.Empty;

    [Required, StringLength(20)]
    public string Platform { get; init; } = "Web";

    [StringLength(200)]
    public string? DeviceName { get; init; }
}
