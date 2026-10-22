using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Attendance;
using EduSync.Domain.Entities;

namespace EduSync.Application.Services.Attendance;

public sealed class AttendanceService(IAttendanceRepository attendances) : IAttendanceService
{
    private static readonly string[] ValidStatuses = ["Present", "Absent", "Late", "Excused"];

    public async Task<SessionAttendanceDto> GetAsync(
        Guid sessionId, Guid lecturerUserId, CancellationToken cancellationToken = default)
    {
        ValidateIds(sessionId, lecturerUserId);
        var session = await GetAuthorizedSessionAsync(sessionId, lecturerUserId, cancellationToken);
        var students = await attendances.ListStudentsAsync(
            sessionId, session.CourseSectionId, cancellationToken);
        return new SessionAttendanceDto(sessionId, session.CourseSectionId, students);
    }

    public Task<SessionAttendanceDto> MarkAsync(
        Guid sessionId,
        Guid lecturerUserId,
        IReadOnlyList<MarkAttendanceItem> items,
        CancellationToken cancellationToken = default)
    {
        ValidateIds(sessionId, lecturerUserId);
        if (items is null or { Count: < 1 or > 500 }
            || items.Any(item => item is null || item.EnrollmentId == Guid.Empty
                || string.IsNullOrWhiteSpace(item.Status)
                || !ValidStatuses.Contains(item.Status, StringComparer.OrdinalIgnoreCase)
                || item.Note?.Trim().Length > 500)
            || items.Select(item => item.EnrollmentId).Distinct().Count() != items.Count)
        {
            throw new AttendanceException(AttendanceError.InvalidData,
                "Attendance items are invalid.");
        }

        return attendances.WithSessionLockAsync(sessionId, async () =>
        {
            var session = await GetAuthorizedSessionAsync(sessionId, lecturerUserId, cancellationToken);
            if (session.Status == "Cancelled")
            {
                throw new AttendanceException(AttendanceError.SessionCancelled,
                    "Cannot mark attendance for a cancelled class session.");
            }

            var students = await attendances.ListStudentsAsync(
                sessionId, session.CourseSectionId, cancellationToken);
            var enrollmentIds = students.Select(student => student.EnrollmentId).ToHashSet();
            if (items.Any(item => !enrollmentIds.Contains(item.EnrollmentId)))
            {
                throw new AttendanceException(AttendanceError.EnrollmentNotInSession,
                    "An enrollment does not belong to this class session.");
            }

            var recordedAt = DateTime.UtcNow;
            var records = items.Select(item => new AttendanceEntity
            {
                EnrollmentId = item.EnrollmentId,
                ClassSessionId = sessionId,
                Status = ValidStatuses.Single(status => status.Equals(
                    item.Status, StringComparison.OrdinalIgnoreCase)),
                Note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim(),
                RecordedAt = recordedAt
            }).ToArray();
            await attendances.SaveAsync(records, cancellationToken);

            var updatedStudents = await attendances.ListStudentsAsync(
                sessionId, session.CourseSectionId, cancellationToken);
            return new SessionAttendanceDto(sessionId, session.CourseSectionId, updatedStudents);
        }, cancellationToken);
    }

    private async Task<AttendanceSessionInfo> GetAuthorizedSessionAsync(
        Guid sessionId, Guid lecturerUserId, CancellationToken cancellationToken)
    {
        var session = await attendances.GetSessionAsync(sessionId, cancellationToken)
            ?? throw new AttendanceException(AttendanceError.SessionNotFound,
                "Class session not found.");
        if (session.LecturerUserId != lecturerUserId)
        {
            throw new AttendanceException(AttendanceError.NotAssigned,
                "Lecturer is not assigned to this class session.");
        }

        return session;
    }

    private static void ValidateIds(Guid sessionId, Guid lecturerUserId)
    {
        if (sessionId == Guid.Empty || lecturerUserId == Guid.Empty)
        {
            throw new AttendanceException(AttendanceError.InvalidData,
                "Class session ID and lecturer user ID are required.");
        }
    }
}
