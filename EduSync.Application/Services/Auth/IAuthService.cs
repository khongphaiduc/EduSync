using EduSync.Application.DTOs.Auth;

namespace EduSync.Application.Services.Auth;

public interface IAuthService
{
    Task<AuthTokensDto> RegisterAsync(
        RegisterStudentCommand command,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<AuthTokensDto> LoginAsync(
        string email,
        string password,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<AuthTokensDto> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    Task<AuthUserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
