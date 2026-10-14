namespace EduSync.Application.DTOs.Students;

public sealed record StudentListItemDto(
    Guid Id,
    string StudentCode,
    string FullName,
    string Email,
    Guid MajorId,
    string MajorName,
    int EnrollmentYear,
    string Status,
    string? AvatarUrl);
