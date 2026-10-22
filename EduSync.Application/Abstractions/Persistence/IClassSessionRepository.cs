using EduSync.Application.DTOs.ClassSessions;
using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface IClassSessionRepository
{
    Task<CourseSectionScheduleInfo?> GetSectionAsync(Guid sectionId, CancellationToken cancellationToken);

    Task<ClassSessionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ClassSessionEntity>> ListAsync(
        Guid sectionId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        CancellationToken cancellationToken);

    Task<bool> LecturerExistsAsync(Guid lecturerId, CancellationToken cancellationToken);

    Task<Guid?> GetLecturerIdForUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LecturerScheduleItemDto>> ListForLecturerAsync(
        Guid lecturerId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ExistingScheduleSession>> FindOverlapsAsync(
        Guid sectionId,
        Guid lecturerId,
        string[] rooms,
        DateTime fromUtc,
        DateTime toUtc,
        Guid? excludeSessionId,
        CancellationToken cancellationToken);

    Task<T> WithScheduleLockAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken);

    Task CreateBatchAsync(IReadOnlyList<ClassSessionEntity> sessions, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(ClassSessionEntity session, CancellationToken cancellationToken);
}
