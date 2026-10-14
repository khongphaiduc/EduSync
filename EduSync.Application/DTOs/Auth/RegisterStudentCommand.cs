namespace EduSync.Application.DTOs.Auth;

public sealed record RegisterStudentCommand(
    string Email,
    string Password,
    string StudentCode,
    string FullName,
    Guid MajorId,
    int EnrollmentYear,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Phone);
