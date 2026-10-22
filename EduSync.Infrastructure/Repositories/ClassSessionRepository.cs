using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.ClassSessions;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class ClassSessionRepository(MyFschoolDemoContext context) : IClassSessionRepository
{
    public Task<CourseSectionScheduleInfo?> GetSectionAsync(Guid sectionId, CancellationToken cancellationToken) =>
        context.CourseSections.AsNoTracking()
            .Where(section => section.Id == sectionId)
            .Select(section => new CourseSectionScheduleInfo(
                section.Id,
                section.LecturerId,
                section.Semester.StartDate,
                section.Semester.EndDate))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ClassSessionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.ClassSessions.AsNoTracking()
            .SingleOrDefaultAsync(session => session.Id == id, cancellationToken);
        return model?.ToDomainEntity();
    }

    public async Task<IReadOnlyList<ClassSessionEntity>> ListAsync(
        Guid sectionId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        CancellationToken cancellationToken)
    {
        var models = await context.ClassSessions.AsNoTracking()
            .Where(session => session.CourseSectionId == sectionId
                && session.StartAt < toExclusiveUtc && session.EndAt > fromUtc)
            .OrderBy(session => session.StartAt)
            .ThenBy(session => session.Id)
            .ToListAsync(cancellationToken);
        return models.Select(model => model.ToDomainEntity()).ToArray();
    }

    public Task<bool> LecturerExistsAsync(Guid lecturerId, CancellationToken cancellationToken) =>
        context.Lecturers.AsNoTracking()
            .AnyAsync(lecturer => lecturer.Id == lecturerId, cancellationToken);

    public Task<Guid?> GetLecturerIdForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Lecturers.AsNoTracking()
            .Where(lecturer => lecturer.UserId == userId)
            .Select(lecturer => (Guid?)lecturer.Id)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<LecturerScheduleItemDto>> ListForLecturerAsync(
        Guid lecturerId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        CancellationToken cancellationToken)
    {
        return await context.ClassSessions.AsNoTracking()
            .Where(session => session.CourseSection.LecturerId == lecturerId
                && session.StartAt < toExclusiveUtc && session.EndAt > fromUtc)
            .OrderBy(session => session.StartAt)
            .ThenBy(session => session.Id)
            .Select(session => new LecturerScheduleItemDto(
                session.Id,
                session.CourseSectionId,
                session.CourseSection.SectionCode,
                session.CourseSection.CourseId,
                session.CourseSection.Course.CourseCode,
                session.CourseSection.Course.CourseName,
                session.CourseSection.SemesterId,
                session.CourseSection.Semester.Name,
                DateTime.SpecifyKind(session.StartAt, DateTimeKind.Utc),
                DateTime.SpecifyKind(session.EndAt, DateTimeKind.Utc),
                session.Room,
                session.Topic,
                session.Status))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExistingScheduleSession>> FindOverlapsAsync(
        Guid sectionId,
        Guid lecturerId,
        string[] rooms,
        DateTime fromUtc,
        DateTime toUtc,
        Guid? excludeSessionId,
        CancellationToken cancellationToken)
    {
        return await context.ClassSessions.AsNoTracking()
            .Where(session => session.Status != "Cancelled"
                && session.StartAt < toUtc && session.EndAt > fromUtc
                && (!excludeSessionId.HasValue || session.Id != excludeSessionId.Value)
                && (session.CourseSectionId == sectionId
                    || session.CourseSection.LecturerId == lecturerId
                    || session.Room != null && rooms.Contains(session.Room.Trim().ToUpper())))
            .Select(session => new ExistingScheduleSession(
                session.Id,
                session.CourseSectionId,
                session.CourseSection.LecturerId,
                session.StartAt,
                session.EndAt,
                session.Room))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<T> WithScheduleLockAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            """
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = 'EduSync.ClassSessions',
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;
            IF @result < 0 THROW 51000, 'Could not acquire schedule lock.', 1;
            """, cancellationToken);
        var result = await action();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task CreateBatchAsync(IReadOnlyList<ClassSessionEntity> sessions, CancellationToken cancellationToken)
    {
        context.ClassSessions.AddRange(sessions.Select(session => session.ToDatabaseModel()));
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
        {
            throw new ClassSessionException(ClassSessionError.SectionNotFound,
                "Course section no longer exists.");
        }
    }

    public async Task<bool> UpdateAsync(ClassSessionEntity session, CancellationToken cancellationToken)
    {
        var updated = await context.ClassSessions
            .Where(existing => existing.Id == session.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(existing => existing.StartAt, session.StartAt)
                .SetProperty(existing => existing.EndAt, session.EndAt)
                .SetProperty(existing => existing.Room, session.Room)
                .SetProperty(existing => existing.Topic, session.Topic)
                .SetProperty(existing => existing.Status, session.Status), cancellationToken);
        return updated == 1;
    }
}
