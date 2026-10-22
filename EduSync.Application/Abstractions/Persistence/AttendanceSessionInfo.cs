namespace EduSync.Application.Abstractions.Persistence;

public sealed record AttendanceSessionInfo(
    Guid Id,
    Guid CourseSectionId,
    Guid LecturerUserId,
    string Status);
