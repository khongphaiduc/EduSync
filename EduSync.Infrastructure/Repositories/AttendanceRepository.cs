using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.DTOs.Attendance;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class AttendanceRepository(MyFschoolDemoContext context) : IAttendanceRepository
{
    public Task<AttendanceSessionInfo?> GetSessionAsync(
        Guid sessionId, CancellationToken cancellationToken) =>
        context.ClassSessions.AsNoTracking()
            .Where(session => session.Id == sessionId)
            .Select(session => new AttendanceSessionInfo(
                session.Id,
                session.CourseSectionId,
                session.CourseSection.Lecturer.UserId,
                session.Status))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<AttendanceStudentDto>> ListStudentsAsync(
        Guid sessionId, Guid sectionId, CancellationToken cancellationToken)
    {
        var rows = await (
            from enrollment in context.Enrollments.AsNoTracking()
            where enrollment.CourseSectionId == sectionId
            join attendance in context.Attendances.AsNoTracking()
                    .Where(item => item.ClassSessionId == sessionId)
                on enrollment.Id equals attendance.EnrollmentId into attendanceGroup
            from attendance in attendanceGroup.DefaultIfEmpty()
            orderby enrollment.Student.StudentCode, enrollment.Id
            select new
            {
                EnrollmentId = enrollment.Id,
                enrollment.StudentId,
                enrollment.Student.StudentCode,
                enrollment.Student.FullName,
                EnrollmentStatus = enrollment.Status,
                AttendanceStatus = attendance == null ? null : attendance.Status,
                Note = attendance == null ? null : attendance.Note,
                RecordedAt = attendance == null ? (DateTime?)null : attendance.RecordedAt
            }).ToArrayAsync(cancellationToken);

        return rows.Select(row => new AttendanceStudentDto(
            row.EnrollmentId,
            row.StudentId,
            row.StudentCode,
            row.FullName,
            row.EnrollmentStatus,
            row.AttendanceStatus,
            row.Note,
            row.RecordedAt is null
                ? null
                : DateTime.SpecifyKind(row.RecordedAt.Value, DateTimeKind.Utc))).ToArray();
    }

    public async Task<T> WithSessionLockAsync<T>(
        Guid sessionId, Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"EduSync.Attendance.{sessionId:N}";
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = {resource},
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;
            IF @result < 0 THROW 51002, 'Could not acquire attendance lock.', 1;
            """, cancellationToken);
        var result = await action();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task SaveAsync(
        IReadOnlyList<AttendanceEntity> attendances, CancellationToken cancellationToken)
    {
        var sessionId = attendances[0].ClassSessionId;
        var enrollmentIds = attendances.Select(item => item.EnrollmentId).ToArray();
        var existing = await context.Attendances
            .Where(item => item.ClassSessionId == sessionId
                && enrollmentIds.Contains(item.EnrollmentId))
            .ToDictionaryAsync(item => item.EnrollmentId, cancellationToken);

        foreach (var attendance in attendances)
        {
            if (existing.TryGetValue(attendance.EnrollmentId, out var model))
            {
                model.Status = attendance.Status;
                model.Note = attendance.Note;
                model.RecordedAt = attendance.RecordedAt;
            }
            else
            {
                context.Attendances.Add(new Attendance
                {
                    Id = Guid.NewGuid(),
                    EnrollmentId = attendance.EnrollmentId,
                    ClassSessionId = attendance.ClassSessionId,
                    Status = attendance.Status,
                    Note = attendance.Note,
                    RecordedAt = attendance.RecordedAt
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
