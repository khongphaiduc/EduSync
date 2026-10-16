namespace EduSync.Application.Abstractions.Persistence;

public sealed record ExistingScheduleSession(
    Guid Id,
    Guid CourseSectionId,
    Guid LecturerId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string? Room);
