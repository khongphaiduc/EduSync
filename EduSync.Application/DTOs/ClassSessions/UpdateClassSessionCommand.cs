namespace EduSync.Application.DTOs.ClassSessions;

public sealed record UpdateClassSessionCommand(
    Guid Id,
    DateTimeOffset? StartAtUtc,
    DateTimeOffset? EndAtUtc,
    string? Room,
    bool ClearRoom,
    string? Topic,
    bool ClearTopic,
    string? Status,
    string TimeZoneId);
