using EduSync.Application.Abstractions.Authentication;
using EduSync.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EduSync.Infrastructure.Authentication;

public sealed class IdentityPasswordService : IPasswordService
{
    private readonly PasswordHasher<UserEntity> _hasher = new();

    public string Hash(UserEntity user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(UserEntity user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
