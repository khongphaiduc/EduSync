namespace EduSync.Application.DTOs.ClassSessions;

public sealed record LecturerScheduleItemDto(
    Guid Id,
    Guid CourseSectionId,
    string SectionCode,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    Guid SemesterId,
    string SemesterName,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string? Room,
    string? Topic,
    string Status);
