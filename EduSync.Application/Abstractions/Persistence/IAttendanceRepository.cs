using EduSync.Application.DTOs.Attendance;
using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface IAttendanceRepository
{
    Task<AttendanceSessionInfo?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AttendanceStudentDto>> ListStudentsAsync(
        Guid sessionId, Guid sectionId, CancellationToken cancellationToken);

    Task<T> WithSessionLockAsync<T>(
        Guid sessionId, Func<Task<T>> action, CancellationToken cancellationToken);

    Task SaveAsync(IReadOnlyList<AttendanceEntity> attendances, CancellationToken cancellationToken);
}
