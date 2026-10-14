namespace EduSync.Application.DTOs.Students;

public sealed record CreateStudentCommand(
    string Email,
    string Password,
    string StudentCode,
    string FullName,
    Guid MajorId,
    int EnrollmentYear,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Phone);
