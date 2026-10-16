using EduSync.Application.Abstractions.Authentication;
using EduSync.Application.Abstractions.Persistence;
using EduSync.Domain.Entities;
using EduSync.Domain.Enums;

namespace EduSync.API.Common;

public static class AdminAccountBootstrap
{
    public static async Task<bool> SeedAsync(
        IConfiguration configuration,
        IAuthRepository accounts,
        IPasswordService passwords,
        CancellationToken cancellationToken = default)
    {
        var email = configuration["Admin:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(email) || email.Length > 255
            || !System.Net.Mail.MailAddress.TryCreate(email, out var address)
            || address.Address != email
            || string.IsNullOrWhiteSpace(password) || password.Length is < 12 or > 128)
        {
            throw new InvalidOperationException(
                "Admin_Email must be a valid email and Admin_Password must be 12 to 128 characters.");
        }

        var existing = await accounts.FindUserByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            if (existing.Role.Name != UserRole.Admin)
            {
                throw new InvalidOperationException(
                    $"Cannot create admin account: {email} is already assigned to another role.");
            }

            return false;
        }

        var adminRole = await accounts.FindRoleAsync(UserRole.Admin, cancellationToken)
            ?? throw new InvalidOperationException("Admin role is not configured.");
        var admin = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            RoleId = adminRole.Id,
            Role = adminRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = passwords.Hash(admin, password);
        await accounts.CreateAccountAsync(admin, null, null, cancellationToken);
        return true;
    }
}
