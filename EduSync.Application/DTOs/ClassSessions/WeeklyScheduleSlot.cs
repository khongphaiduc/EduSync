namespace EduSync.Application.DTOs.ClassSessions;

public sealed record WeeklyScheduleSlot(
    string DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Room);
