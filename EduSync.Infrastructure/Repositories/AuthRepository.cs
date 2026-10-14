using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class AuthRepository(MyFschoolDemoContext context) : IAuthRepository
{
    public async Task<UserEntity?> FindUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var model = await context.Users.AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        return model?.ToDomainEntity();
    }

    public async Task<UserEntity?> FindUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.Users.AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
        return model?.ToDomainEntity();
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<bool> StudentCodeExistsAsync(string studentCode, CancellationToken cancellationToken) =>
        context.Students.AnyAsync(student => student.StudentCode == studentCode, cancellationToken);

    public Task<bool> MajorExistsAsync(Guid majorId, CancellationToken cancellationToken) =>
        context.Majors.AnyAsync(major => major.Id == majorId, cancellationToken);

    public async Task CreateAccountAsync(
        UserEntity user,
        StudentEntity? student,
        RefreshTokenEntity refreshToken,
        CancellationToken cancellationToken)
    {
        context.Users.Add(user.ToDatabaseModel());
        if (student is not null)
        {
            context.Students.Add(student.ToDatabaseModel());
        }

        context.RefreshTokens.Add(refreshToken.ToDatabaseModel());
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new AuthException(AuthError.RegistrationConflict, "Email or student code already exists.");
        }
    }

    public async Task AddRefreshTokenAsync(RefreshTokenEntity token, CancellationToken cancellationToken)
    {
        context.RefreshTokens.Add(token.ToDatabaseModel());
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshSession?> FindRefreshSessionAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        var model = await context.RefreshTokens.AsNoTracking()
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        return model is null
            ? null
            : new RefreshSession(model.User.ToDomainEntity(), model.ToDomainEntity());
    }

    public async Task<bool> RotateRefreshTokenAsync(
        RefreshTokenEntity previous,
        RefreshTokenEntity next,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        context.RefreshTokens.Add(next.ToDatabaseModel());
        await context.SaveChangesAsync(cancellationToken);

        var updated = await context.RefreshTokens
            .Where(token => token.Id == previous.Id
                && token.UserId == next.UserId
                && token.RevokedAt == null
                && token.ExpiresAt > nowUtc
                && token.User.IsActive)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(token => token.RevokedAt, (DateTime?)nowUtc)
                .SetProperty(token => token.ReplacedByTokenId, (Guid?)next.Id), cancellationToken);

        if (updated != 1)
        {
            return false;
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RevokeRefreshTokenAsync(
        string tokenHash,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var updated = await context.RefreshTokens
            .Where(token => token.TokenHash == tokenHash
                && token.RevokedAt == null
                && token.ExpiresAt > nowUtc)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(token => token.RevokedAt, (DateTime?)nowUtc), cancellationToken);
        return updated == 1;
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
