using Xunit;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Auth;
using EduSync.Application.Services.Auth;
using EduSync.Domain.Entities;
using EduSync.Domain.Enums;
using EduSync.Infrastructure.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace EduSync.Tests;

public sealed class AuthServiceTests
{
    private static readonly Guid MajorId = Guid.NewGuid();
    private static readonly Guid StudentRoleId = Guid.NewGuid();
    private static readonly JwtOptions Jwt = new()
    {
        Issuer = "EduSync.Tests",
        Audience = "EduSync.Tests.Client",
        Key = "test-only-secret-with-at-least-32-bytes",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 30
    };

    [Fact]
    public async Task Register_CreatesStudentAndIssuesValidTokens()
    {
        var repository = new FakeAuthRepository(MajorId);
        var passwords = new IdentityPasswordService();
        var service = new AuthService(repository, passwords, new JwtTokenService(Jwt));

        var result = await service.RegisterAsync(Command(), "127.0.0.1", CancellationToken.None);

        Assert.Equal("student@example.com", repository.User?.Email);
        Assert.Equal(StudentRoleId, repository.User?.RoleId);
        Assert.Equal(UserRole.Student, repository.User?.Role.Name);
        Assert.Equal(UserRole.Student, result.Role);
        Assert.Equal(repository.User?.Id, repository.Student?.UserId);
        Assert.NotEqual(Command().Password, repository.User?.PasswordHash);
        Assert.True(passwords.Verify(repository.User!, Command().Password));
        Assert.NotEqual(result.RefreshToken, repository.RefreshToken?.TokenHash);
        Assert.Equal(64, repository.RefreshToken?.TokenHash.Length);
        Assert.True(result.RefreshTokenExpiresAtUtc > result.AccessTokenExpiresAtUtc);

        var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(
            result.AccessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = Jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            },
            out _);
        Assert.Equal(result.UserId.ToString(), principal.FindFirst("sub")?.Value);
        Assert.Equal("Student", principal.FindFirst("role")?.Value);
    }

    [Fact]
    public async Task Login_RejectsWrongPassword()
    {
        var repository = new FakeAuthRepository(MajorId);
        var service = new AuthService(repository, new IdentityPasswordService(), new JwtTokenService(Jwt));
        await service.RegisterAsync(Command(), null, CancellationToken.None);

        var error = await Assert.ThrowsAsync<AuthException>(() => service.LoginAsync(
            "student@example.com", "wrong-password", null, CancellationToken.None));

        Assert.Equal(AuthError.InvalidCredentials, error.Error);
    }

    [Fact]
    public async Task Refresh_RotatesTokenAndRejectsReuse()
    {
        var repository = new FakeAuthRepository(MajorId);
        var service = new AuthService(repository, new IdentityPasswordService(), new JwtTokenService(Jwt));
        var first = await service.RegisterAsync(Command(), null, CancellationToken.None);

        var second = await service.RefreshAsync(first.RefreshToken, null, CancellationToken.None);

        Assert.NotEqual(first.RefreshToken, second.RefreshToken);
        var error = await Assert.ThrowsAsync<AuthException>(() =>
            service.RefreshAsync(first.RefreshToken, null, CancellationToken.None));
        Assert.Equal(AuthError.InvalidRefreshToken, error.Error);
    }

    [Fact]
    public async Task Login_IssuesNewTokenPair()
    {
        var repository = new FakeAuthRepository(MajorId);
        var service = new AuthService(repository, new IdentityPasswordService(), new JwtTokenService(Jwt));
        var registration = await service.RegisterAsync(Command(), null, CancellationToken.None);

        var login = await service.LoginAsync(
            "STUDENT@example.com", Command().Password, null, CancellationToken.None);

        Assert.Equal(registration.UserId, login.UserId);
        Assert.NotEqual(registration.AccessToken, login.AccessToken);
        Assert.NotEqual(registration.RefreshToken, login.RefreshToken);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var repository = new FakeAuthRepository(MajorId);
        var service = new AuthService(repository, new IdentityPasswordService(), new JwtTokenService(Jwt));
        var registration = await service.RegisterAsync(Command(), null, CancellationToken.None);

        await service.LogoutAsync(registration.RefreshToken, CancellationToken.None);

        var error = await Assert.ThrowsAsync<AuthException>(() =>
            service.RefreshAsync(registration.RefreshToken, null, CancellationToken.None));
        Assert.Equal(AuthError.InvalidRefreshToken, error.Error);
    }

    private static RegisterStudentCommand Command() => new(
        "Student@Example.com", "Password123!", "STU001", "Test Student", MajorId,
        2026, null, null, null);

    internal sealed class FakeAuthRepository(Guid majorId) : IAuthRepository
    {
        private readonly Dictionary<string, RefreshTokenEntity> _tokens = [];

        public UserEntity? User { get; private set; }
        public StudentEntity? Student { get; private set; }
        public RefreshTokenEntity? RefreshToken { get; private set; }

        public Task<UserEntity?> FindUserByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(User?.Email == email ? User : null);

        public Task<UserEntity?> FindUserByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(User?.Id == id ? User : null);

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(User?.Email == email);

        public Task<bool> StudentCodeExistsAsync(string studentCode, CancellationToken cancellationToken) =>
            Task.FromResult(Student?.StudentCode == studentCode);

        public Task<bool> MajorExistsAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(id == majorId);

        public Task<RoleEntity?> FindRoleAsync(UserRole role, CancellationToken cancellationToken) =>
            Task.FromResult<RoleEntity?>(role == UserRole.Student
                ? new RoleEntity { Id = StudentRoleId, Name = UserRole.Student }
                : null);

        public Task CreateAccountAsync(
            UserEntity user,
            StudentEntity? student,
            RefreshTokenEntity? refreshToken,
            CancellationToken cancellationToken)
        {
            User = user;
            Student = student;
            RefreshToken = refreshToken;
            if (refreshToken is not null)
            {
                _tokens.Add(refreshToken.TokenHash, refreshToken);
            }
            return Task.CompletedTask;
        }

        public Task AddRefreshTokenAsync(RefreshTokenEntity token, CancellationToken cancellationToken)
        {
            RefreshToken = token;
            _tokens.Add(token.TokenHash, token);
            return Task.CompletedTask;
        }

        public Task<RefreshSession?> FindRefreshSessionAsync(string hash, CancellationToken cancellationToken) =>
            Task.FromResult(_tokens.TryGetValue(hash, out var token) && User is not null
                ? new RefreshSession(User, token)
                : null);

        public Task<bool> RotateRefreshTokenAsync(
            RefreshTokenEntity previous,
            RefreshTokenEntity next,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            if (previous.RevokedAt is not null || previous.ExpiresAt <= nowUtc)
            {
                return Task.FromResult(false);
            }

            previous.RevokedAt = nowUtc;
            previous.ReplacedByTokenId = next.Id;
            _tokens.Add(next.TokenHash, next);
            RefreshToken = next;
            return Task.FromResult(true);
        }

        public Task<bool> RevokeRefreshTokenAsync(string hash, DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (!_tokens.TryGetValue(hash, out var token) || token.RevokedAt is not null)
            {
                return Task.FromResult(false);
            }

            token.RevokedAt = nowUtc;
            return Task.FromResult(true);
        }
    }
}
