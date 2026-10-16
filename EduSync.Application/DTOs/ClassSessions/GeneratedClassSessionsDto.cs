namespace EduSync.Application.DTOs.ClassSessions;

public sealed record GeneratedClassSessionsDto(
    Guid CourseSectionId,
    int CreatedCount,
    IReadOnlyList<ClassSessionDto> Sessions);
