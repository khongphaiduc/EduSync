using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface IAuthRepository
{
    Task<UserEntity?> FindUserByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserEntity?> FindUserByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<bool> StudentCodeExistsAsync(string studentCode, CancellationToken cancellationToken);

    Task<bool> MajorExistsAsync(Guid majorId, CancellationToken cancellationToken);

    Task CreateAccountAsync(
        UserEntity user,
        StudentEntity? student,
        RefreshTokenEntity refreshToken,
        CancellationToken cancellationToken);

    Task AddRefreshTokenAsync(RefreshTokenEntity token, CancellationToken cancellationToken);

    Task<RefreshSession?> FindRefreshSessionAsync(string tokenHash, CancellationToken cancellationToken);

    Task<bool> RotateRefreshTokenAsync(
        RefreshTokenEntity previous,
        RefreshTokenEntity next,
        DateTime nowUtc,
        CancellationToken cancellationToken);

    Task<bool> RevokeRefreshTokenAsync(string tokenHash, DateTime nowUtc, CancellationToken cancellationToken);
}
