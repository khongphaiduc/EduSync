using EduSync.Application.DTOs.Attendance;

namespace EduSync.Application.Services.Attendance;

public interface IAttendanceService
{
    Task<SessionAttendanceDto> GetAsync(
        Guid sessionId, Guid lecturerUserId, CancellationToken cancellationToken = default);

    Task<SessionAttendanceDto> MarkAsync(
        Guid sessionId,
        Guid lecturerUserId,
        IReadOnlyList<MarkAttendanceItem> items,
        CancellationToken cancellationToken = default);
}
