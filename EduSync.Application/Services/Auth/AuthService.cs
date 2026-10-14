using EduSync.Application.Abstractions.Authentication;
using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Auth;
using EduSync.Domain.Entities;
using EduSync.Domain.Enums;

namespace EduSync.Application.Services.Auth;

public sealed class AuthService(
    IAuthRepository repository,
    IPasswordService passwords,
    IAuthTokenService tokens) : IAuthService
{
    public async Task<AuthTokensDto> RegisterAsync(
        RegisterStudentCommand command,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(command.Email);
        var studentCode = command.StudentCode.Trim();
        var fullName = command.FullName.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(studentCode)
            || string.IsNullOrWhiteSpace(fullName)
            || command.Password.Length is < 8 or > 128
            || command.EnrollmentYear is < 1900
            || command.EnrollmentYear > DateTime.UtcNow.Year + 1)
        {
            throw new AuthException(AuthError.InvalidRegistration, "Registration data is invalid.");
        }

        if (command.MajorId == Guid.Empty
            || !await repository.MajorExistsAsync(command.MajorId, cancellationToken))
        {
            throw new AuthException(AuthError.InvalidMajor, "Major does not exist.");
        }

        if (await repository.EmailExistsAsync(email, cancellationToken))
        {
            throw new AuthException(AuthError.EmailAlreadyExists, "Email already exists.");
        }

        if (await repository.StudentCodeExistsAsync(studentCode, cancellationToken))
        {
            throw new AuthException(AuthError.StudentCodeAlreadyExists, "Student code already exists.");
        }

        var studentRole = await repository.FindRoleAsync(UserRole.Student, cancellationToken)
            ?? throw new InvalidOperationException("Student role is not configured.");

        var nowUtc = DateTime.UtcNow;
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            RoleId = studentRole.Id,
            Role = studentRole,
            IsActive = true,
            CreatedAt = nowUtc
        };
        user.PasswordHash = passwords.Hash(user, command.Password);
        var student = new StudentEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StudentCode = studentCode,
            FullName = fullName,
            MajorId = command.MajorId,
            EnrollmentYear = command.EnrollmentYear,
            DateOfBirth = command.DateOfBirth,
            Gender = command.Gender,
            Phone = command.Phone,
            Status = "Studying"
        };
        var issued = tokens.Issue(user);
        await repository.CreateAccountAsync(
            user,
            student,
            CreateRefreshToken(user.Id, issued, ipAddress),
            cancellationToken);
        return ToDto(user, issued);
    }

    public async Task<AuthTokensDto> LoginAsync(
        string email,
        string password,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var user = await repository.FindUserByEmailAsync(NormalizeEmail(email), cancellationToken);
        if (user is null || !passwords.Verify(user, password))
        {
            throw new AuthException(AuthError.InvalidCredentials, "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new AuthException(AuthError.InactiveAccount, "Account is inactive.");
        }

        var issued = tokens.Issue(user);
        await repository.AddRefreshTokenAsync(
            CreateRefreshToken(user.Id, issued, ipAddress),
            cancellationToken);
        return ToDto(user, issued);
    }

    public async Task<AuthTokensDto> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var hash = tokens.HashRefreshToken(refreshToken);
        var session = await repository.FindRefreshSessionAsync(hash, cancellationToken);
        var nowUtc = DateTime.UtcNow;
        if (session is null || session.Token.RevokedAt is not null
            || session.Token.ExpiresAt <= nowUtc || !session.User.IsActive)
        {
            throw new AuthException(AuthError.InvalidRefreshToken, "Refresh token is invalid or expired.");
        }

        var issued = tokens.Issue(session.User);
        var next = CreateRefreshToken(
            session.User.Id,
            issued,
            ipAddress);
        var rotated = await repository.RotateRefreshTokenAsync(
            session.Token, next, nowUtc, cancellationToken);
        if (!rotated)
        {
            throw new AuthException(AuthError.InvalidRefreshToken, "Refresh token is invalid or expired.");
        }

        return ToDto(session.User, issued);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = tokens.HashRefreshToken(refreshToken);
        await repository.RevokeRefreshTokenAsync(hash, DateTime.UtcNow, cancellationToken);
    }

    public async Task<AuthUserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new AuthException(AuthError.InactiveAccount, "Account is inactive.");
        }

        return new AuthUserDto(user.Id, user.Email, user.Role.Name);
    }

    private RefreshTokenEntity CreateRefreshToken(
        Guid userId,
        IssuedTokens issued,
        string? ipAddress) => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokens.HashRefreshToken(issued.RefreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = issued.RefreshTokenExpiresAtUtc,
            CreatedByIp = ipAddress
        };

    private static AuthTokensDto ToDto(UserEntity user, IssuedTokens issued) => new(
        user.Id,
        user.Email,
        user.Role.Name,
        issued.AccessToken,
        issued.AccessTokenExpiresAtUtc,
        issued.RefreshToken,
        issued.RefreshTokenExpiresAtUtc);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
