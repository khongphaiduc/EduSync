using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
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
