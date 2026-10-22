using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.DTOs.GradeComponents;
using EduSync.Application.DTOs.Grades;
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
}
