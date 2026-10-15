using EduSync.Application.Abstractions.Authentication;
using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Lecturers;
using EduSync.Domain.Entities;
using EduSync.Domain.Enums;

namespace EduSync.Application.Services.Lecturers;

public sealed class LecturerService(
    ILecturerRepository lecturers,
    IAuthRepository accounts,
    IPasswordService passwords) : ILecturerService
{
    public async Task<LecturerCreatedDto> CreateAsync(
        CreateLecturerCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var lecturerCode = command.LecturerCode.Trim();
        var fullName = command.FullName.Trim();
        var phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim();
        if (string.IsNullOrWhiteSpace(email) || email.Length > 255
            || string.IsNullOrWhiteSpace(lecturerCode) || lecturerCode.Length > 30
            || string.IsNullOrWhiteSpace(fullName) || fullName.Length > 150
            || command.Password.Length is < 8 or > 128
            || phone?.Length > 20)
        {
            throw new AuthException(AuthError.InvalidRegistration, "Lecturer data is invalid.");
        }

        if (await accounts.EmailExistsAsync(email, cancellationToken))
        {
            throw new AuthException(AuthError.EmailAlreadyExists, "Email already exists.");
        }

        if (await lecturers.LecturerCodeExistsAsync(lecturerCode, cancellationToken))
        {
            throw new AuthException(AuthError.LecturerCodeAlreadyExists, "Lecturer code already exists.");
        }

        var lecturerRole = await accounts.FindRoleAsync(UserRole.Lecturer, cancellationToken)
            ?? throw new InvalidOperationException("Lecturer role is not configured.");
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            RoleId = lecturerRole.Id,
            Role = lecturerRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwords.Hash(user, command.Password);
        var lecturer = new LecturerEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LecturerCode = lecturerCode,
            FullName = fullName,
            Phone = phone
        };

        await lecturers.CreateAsync(user, lecturer, cancellationToken);
        return new LecturerCreatedDto(
            lecturer.Id,
            user.Id,
            lecturer.LecturerCode,
            lecturer.FullName,
            user.Email,
            lecturer.Phone,
            user.IsActive);
    }

    public Task<LecturerListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }

        return lecturers.ListAsync(page, pageSize, cancellationToken);
    }
}
