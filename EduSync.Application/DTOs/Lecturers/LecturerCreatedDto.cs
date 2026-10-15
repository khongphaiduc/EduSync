namespace EduSync.Application.DTOs.Lecturers;

public sealed record LecturerCreatedDto(
    Guid Id,
    Guid UserId,
    string LecturerCode,
    string FullName,
    string Email,
    string? Phone,
    bool IsActive);
