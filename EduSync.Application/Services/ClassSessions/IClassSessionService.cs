using EduSync.Application.DTOs.ClassSessions;

namespace EduSync.Application.Services.ClassSessions;

public interface IClassSessionService
{
    Task<GeneratedClassSessionsDto> GenerateAsync(
        GenerateClassSessionsCommand command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassSessionDto>> ListAsync(
        Guid courseSectionId,
        DateOnly fromDate,
        DateOnly toDate,
        string timeZoneId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LecturerScheduleItemDto>> ListForLecturerAsync(
        Guid lecturerId,
        DateOnly fromDate,
        DateOnly toDate,
        string timeZoneId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LecturerScheduleItemDto>> ListForCurrentLecturerAsync(
        Guid userId,
        DateOnly fromDate,
        DateOnly toDate,
        string timeZoneId,
        CancellationToken cancellationToken = default);

    Task<ClassSessionDto> UpdateAsync(
        UpdateClassSessionCommand command,
        CancellationToken cancellationToken = default);
}
