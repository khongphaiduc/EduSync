using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.GradeComponents;
using EduSync.Application.DTOs.Grades;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class SectionGradeRepository(MyFschoolDemoContext context) : ISectionGradeRepository
{
    public Task<GradeSectionInfo?> GetSectionAsync(Guid sectionId, CancellationToken cancellationToken) =>
        context.CourseSections.AsNoTracking()
            .Where(section => section.Id == sectionId)
            .Select(section => new GradeSectionInfo(section.Id, section.Lecturer.UserId))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<SectionGradeListDto> ListAsync(
        Guid sectionId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var components = await context.GradeComponents.AsNoTracking()
            .Where(component => component.CourseSectionId == sectionId)
            .OrderBy(component => component.Name)
            .ThenBy(component => component.Id)
            .Select(component => new GradeComponentDto(
                component.Id,
                component.CourseSectionId,
                component.Name,
                component.Weight,
                component.MaxScore))
            .ToArrayAsync(cancellationToken);

        var enrollments = context.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.CourseSectionId == sectionId);
        var totalCount = await enrollments.CountAsync(cancellationToken);
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new SectionGradeListDto(sectionId, components, [], page, pageSize, totalCount);
        }

        var studentRows = await enrollments
            .OrderBy(enrollment => enrollment.Student.StudentCode)
            .ThenBy(enrollment => enrollment.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(enrollment => new
            {
                EnrollmentId = enrollment.Id,
                enrollment.StudentId,
                enrollment.Student.StudentCode,
                enrollment.Student.FullName,
                EnrollmentStatus = enrollment.Status
            })
            .ToArrayAsync(cancellationToken);
        if (studentRows.Length == 0)
        {
            return new SectionGradeListDto(sectionId, components, [], page, pageSize, totalCount);
        }

        var enrollmentIds = studentRows.Select(student => student.EnrollmentId).ToArray();
        var gradeRows = await context.Grades.AsNoTracking()
            .Where(grade => enrollmentIds.Contains(grade.EnrollmentId)
                && grade.GradeComponent.CourseSectionId == sectionId)
            .Select(grade => new
            {
                grade.Id,
                grade.EnrollmentId,
                grade.GradeComponentId,
                grade.Score,
                grade.GradedAt,
                grade.UpdatedAt
            })
            .ToArrayAsync(cancellationToken);
        var gradesByKey = gradeRows.ToDictionary(
            grade => (grade.EnrollmentId, grade.GradeComponentId));

        var students = studentRows.Select(student => new StudentGradesDto(
            student.EnrollmentId,
            student.StudentId,
            student.StudentCode,
            student.FullName,
            student.EnrollmentStatus,
            components.Select(component =>
            {
                if (!gradesByKey.TryGetValue((student.EnrollmentId, component.Id), out var grade))
                {
                    return new StudentGradeDto(component.Id, null, null, null, null);
                }

                return new StudentGradeDto(
                    component.Id,
                    grade.Id,
                    grade.Score,
                    grade.GradedAt is null
                        ? null
                        : DateTime.SpecifyKind(grade.GradedAt.Value, DateTimeKind.Utc),
                    grade.UpdatedAt is null
                        ? null
                        : DateTime.SpecifyKind(grade.UpdatedAt.Value, DateTimeKind.Utc));
            }).ToArray())).ToArray();

        return new SectionGradeListDto(sectionId, components, students, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<SavedGradeDto>> SaveAsync(
        Guid sectionId, IReadOnlyList<SaveGradeItem> items, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"EduSync.Grades.{sectionId:N}";
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = {resource},
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;
            IF @result < 0 THROW 51003, 'Could not acquire grade lock.', 1;
            """, cancellationToken);

        var enrollmentIds = items.Select(item => item.EnrollmentId).Distinct().ToArray();
        var componentIds = items.Select(item => item.GradeComponentId).Distinct().ToArray();
        var validEnrollmentIds = await context.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.CourseSectionId == sectionId
                && enrollmentIds.Contains(enrollment.Id))
            .Select(enrollment => enrollment.Id)
            .ToArrayAsync(cancellationToken);
        if (validEnrollmentIds.Length != enrollmentIds.Length)
        {
            throw new GradeException(GradeError.EnrollmentNotInSection,
                "An enrollment does not belong to this course section.");
        }

        var componentScores = await context.GradeComponents.AsNoTracking()
            .Where(component => component.CourseSectionId == sectionId
                && componentIds.Contains(component.Id))
            .Select(component => new { component.Id, component.MaxScore })
            .ToDictionaryAsync(component => component.Id, component => component.MaxScore, cancellationToken);
        if (componentScores.Count != componentIds.Length)
        {
            throw new GradeException(GradeError.ComponentNotInSection,
                "A grade component does not belong to this course section.");
        }

        if (items.Any(item => item.Score > componentScores[item.GradeComponentId]))
        {
            throw new GradeException(GradeError.InvalidData,
                "A score exceeds the grade component maximum.");
        }

        var existing = await context.Grades
            .Where(grade => enrollmentIds.Contains(grade.EnrollmentId)
                && componentIds.Contains(grade.GradeComponentId))
            .ToDictionaryAsync(grade => (grade.EnrollmentId, grade.GradeComponentId), cancellationToken);
        var now = DateTime.UtcNow;
        var result = new List<SavedGradeDto>(items.Count);
        foreach (var item in items)
        {
            if (!existing.TryGetValue((item.EnrollmentId, item.GradeComponentId), out var grade))
            {
                if (item.Score is null)
                {
                    result.Add(new SavedGradeDto(item.EnrollmentId, item.GradeComponentId,
                        null, null, null, null));
                    continue;
                }

                var entity = new GradeEntity
                {
                    Id = Guid.NewGuid(),
                    EnrollmentId = item.EnrollmentId,
                    GradeComponentId = item.GradeComponentId,
                    Score = item.Score,
                    GradedAt = now,
                    UpdatedAt = now
                };
                grade = entity.ToDatabaseModel();
                context.Grades.Add(grade);
            }
            else
            {
                grade.Score = item.Score;
                grade.GradedAt = item.Score is null ? null : grade.GradedAt ?? now;
                grade.UpdatedAt = now;
            }

            result.Add(new SavedGradeDto(item.EnrollmentId, item.GradeComponentId,
                grade.Id, grade.Score, grade.GradedAt is null ? null :
                    DateTime.SpecifyKind(grade.GradedAt.Value, DateTimeKind.Utc),
                DateTime.SpecifyKind(grade.UpdatedAt!.Value, DateTimeKind.Utc)));
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
