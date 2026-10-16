namespace EduSync.Application.DTOs.ClassSessions;

public sealed record ClassSessionDto(
    Guid Id,
    Guid CourseSectionId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string? Room,
    string? Topic,
    string Status);
