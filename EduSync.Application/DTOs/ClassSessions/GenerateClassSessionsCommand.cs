namespace EduSync.Application.DTOs.ClassSessions;

public sealed record GenerateClassSessionsCommand(
    Guid CourseSectionId,
    DateOnly FromDate,
    DateOnly ToDate,
    string TimeZoneId,
    IReadOnlyList<WeeklyScheduleSlot> WeeklySlots,
    IReadOnlyList<DateOnly> ExcludeDates);
