namespace EduSync.Application.DTOs.ClassSessions;

public sealed record StudentScheduleItemDto(
    Guid Id,
    Guid CourseSectionId,
    string SectionCode,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    Guid SemesterId,
    string SemesterName,
    Guid LecturerId,
    string LecturerName,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string? Room,
    string? Topic,
    string Status);
