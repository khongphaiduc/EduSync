using EduSync.Application.Abstractions.Authentication;
using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Students;
using EduSync.Domain.Entities;
using EduSync.Domain.Enums;

namespace EduSync.Application.Services.Students;

public sealed class StudentService(
    IStudentRepository students,
    IAuthRepository accounts,
    IPasswordService passwords) : IStudentService
{
    public async Task<StudentCreatedDto> CreateAsync(
        CreateStudentCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var studentCode = command.StudentCode.Trim();
        var fullName = command.FullName.Trim();
        if (string.IsNullOrWhiteSpace(email) || email.Length > 255
            || string.IsNullOrWhiteSpace(studentCode) || studentCode.Length > 30
            || string.IsNullOrWhiteSpace(fullName) || fullName.Length > 150
            || command.Password.Length is < 8 or > 128
            || command.EnrollmentYear is < 1900
            || command.EnrollmentYear > DateTime.UtcNow.Year + 1)
        {
            throw new AuthException(AuthError.InvalidRegistration, "Student data is invalid.");
        }

        if (command.MajorId == Guid.Empty
            || !await accounts.MajorExistsAsync(command.MajorId, cancellationToken))
        {
            throw new AuthException(AuthError.InvalidMajor, "Major does not exist.");
        }

        if (await accounts.EmailExistsAsync(email, cancellationToken))
        {
            throw new AuthException(AuthError.EmailAlreadyExists, "Email already exists.");
        }

        if (await accounts.StudentCodeExistsAsync(studentCode, cancellationToken))
        {
            throw new AuthException(AuthError.StudentCodeAlreadyExists, "Student code already exists.");
        }

        var studentRole = await accounts.FindRoleAsync(UserRole.Student, cancellationToken)
            ?? throw new InvalidOperationException("Student role is not configured.");
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            RoleId = studentRole.Id,
            Role = studentRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
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

        await accounts.CreateAccountAsync(user, student, null, cancellationToken);
        return new StudentCreatedDto(
            student.Id,
            user.Id,
            student.StudentCode,
            student.FullName,
            user.Email,
            student.MajorId,
            student.EnrollmentYear,
            student.Status);
    }

    public Task<StudentListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }

        return students.ListAsync(page, pageSize, cancellationToken);
    }
}
