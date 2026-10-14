namespace EduSync.Application.DTOs.Students;

public sealed record StudentCreatedDto(
    Guid Id,
    Guid UserId,
    string StudentCode,
    string FullName,
    string Email,
    Guid MajorId,
    int EnrollmentYear,
    string Status);
