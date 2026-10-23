using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.CourseSections;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class CourseSectionRepository(MyFschoolDemoContext context) : ICourseSectionRepository
{
    public Task<CourseSectionListDto> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ListAsync(context.CourseSections.AsNoTracking(), page, pageSize, cancellationToken);

    public Task<CourseSectionListDto> ListForLecturerAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ListAsync(context.CourseSections.AsNoTracking()
            .Where(section => section.Lecturer.UserId == userId),
            page, pageSize, cancellationToken);

    public Task<bool> StudentExistsForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Students.AsNoTracking()
            .AnyAsync(student => student.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<StudentSemesterCourseDto>> ListForStudentSemesterAsync(
        Guid userId,
        Guid semesterId,
        CancellationToken cancellationToken)
    {
        return await context.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.Student.UserId == userId
                && enrollment.CourseSection.SemesterId == semesterId
                && enrollment.Status == "Registered")
            .OrderBy(enrollment => enrollment.CourseSection.Course.CourseCode)
            .ThenBy(enrollment => enrollment.CourseSection.SectionCode)
            .ThenBy(enrollment => enrollment.Id)
            .Select(enrollment => new StudentSemesterCourseDto(
                enrollment.Id,
                enrollment.Status,
                enrollment.CourseSection.CourseId,
                enrollment.CourseSection.Course.CourseCode,
                enrollment.CourseSection.Course.CourseName,
                enrollment.CourseSection.Course.Credits,
                enrollment.CourseSectionId,
                enrollment.CourseSection.SectionCode,
                enrollment.CourseSection.Status,
                enrollment.CourseSection.SemesterId,
                enrollment.CourseSection.Semester.Name,
                enrollment.CourseSection.Semester.AcademicYear,
                enrollment.CourseSection.LecturerId,
                enrollment.CourseSection.Lecturer.FullName))
            .ToArrayAsync(cancellationToken);
    }

    private static async Task<CourseSectionListDto> ListAsync(
        IQueryable<CourseSection> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new CourseSectionListDto([], page, pageSize, totalCount);
        }

        var items = await query
            .OrderByDescending(section => section.Semester.StartDate)
            .ThenBy(section => section.SectionCode)
            .ThenBy(section => section.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(section => new CourseSectionListItemDto(
                section.Id,
                section.CourseId,
                section.Course.CourseCode,
                section.Course.CourseName,
                section.SemesterId,
                section.Semester.Name,
                section.Semester.AcademicYear,
                section.LecturerId,
                section.Lecturer.FullName,
                section.SectionCode,
                section.Capacity,
                section.Status))
            .ToArrayAsync(cancellationToken);

        return new CourseSectionListDto(items, page, pageSize, totalCount);
    }

    public Task<bool> CourseExistsAsync(Guid courseId, CancellationToken cancellationToken) =>
        context.Courses.AnyAsync(course => course.Id == courseId, cancellationToken);

    public Task<bool> SemesterExistsAsync(Guid semesterId, CancellationToken cancellationToken) =>
        context.Semesters.AnyAsync(semester => semester.Id == semesterId, cancellationToken);

    public Task<bool> LecturerExistsAsync(Guid lecturerId, CancellationToken cancellationToken) =>
        context.Lecturers.AnyAsync(lecturer => lecturer.Id == lecturerId, cancellationToken);

    public Task<bool> SectionCodeExistsAsync(Guid semesterId, string sectionCode, CancellationToken cancellationToken) =>
        context.CourseSections.AnyAsync(
            section => section.SemesterId == semesterId && section.SectionCode == sectionCode,
            cancellationToken);

    public async Task CreateAsync(CourseSectionEntity section, CancellationToken cancellationToken)
    {
        context.CourseSections.Add(section.ToDatabaseModel());
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new CourseSectionException(CourseSectionError.AlreadyExists,
                "Section code already exists in this semester.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
        {
            throw new CourseSectionException(CourseSectionError.InvalidData,
                "Course, semester, or lecturer no longer exists.");
        }
    }
}
