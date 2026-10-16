using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.DTOs.Lecturers;
using EduSync.Application.Services.Lecturers;
using EduSync.Domain.Enums;

namespace EduSync.API.Common;

public static class LecturerAccountBootstrap
{
    public static async Task<bool> SeedAsync(
        IConfiguration configuration,
        IAuthRepository accounts,
        ILecturerService lecturers,
        CancellationToken cancellationToken = default)
    {
        var email = configuration["Lecturer:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["Lecturer:Password"];
        var code = configuration["Lecturer:Code"]?.Trim();
        var fullName = configuration["Lecturer:FullName"]?.Trim();
        var phone = configuration["Lecturer:Phone"];
        if (string.IsNullOrWhiteSpace(email)
            && string.IsNullOrWhiteSpace(password)
            && string.IsNullOrWhiteSpace(code)
            && string.IsNullOrWhiteSpace(fullName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(email) || email.Length > 255
            || !System.Net.Mail.MailAddress.TryCreate(email, out var address)
            || address.Address != email
            || string.IsNullOrWhiteSpace(password) || password.Length is < 12 or > 128
            || string.IsNullOrWhiteSpace(code) || code.Length > 30
            || string.IsNullOrWhiteSpace(fullName) || fullName.Length > 150
            || phone?.Length > 20)
        {
            throw new InvalidOperationException("Lecturer bootstrap settings are invalid.");
        }

        var existing = await accounts.FindUserByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            if (existing.Role.Name != UserRole.Lecturer)
            {
                throw new InvalidOperationException(
                    $"Cannot create lecturer account: {email} is assigned to another role.");
            }

            return false;
        }

        await lecturers.CreateAsync(
            new CreateLecturerCommand(email, password, code, fullName, phone),
            cancellationToken);
        return true;
    }
}
