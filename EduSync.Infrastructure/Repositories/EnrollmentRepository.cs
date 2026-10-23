using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Enrollments;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class EnrollmentRepository(MyFschoolDemoContext context) : IEnrollmentRepository
{
    public async Task<SectionStudentListDto> ListStudentsAsync(
        Guid sectionId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.CourseSectionId == sectionId);
        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new SectionStudentListDto([], page, pageSize, totalCount);
        }

        var items = await query
            .OrderBy(enrollment => enrollment.Student.StudentCode)
            .ThenBy(enrollment => enrollment.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(enrollment => new SectionStudentListItemDto(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.Student.StudentCode,
                enrollment.Student.FullName,
                enrollment.Student.User.Email,
                enrollment.Student.Status,
                enrollment.Status,
                enrollment.EnrolledAt))
            .ToArrayAsync(cancellationToken);

        return new SectionStudentListDto(items, page, pageSize, totalCount);
    }

    public async Task<T> WithSectionLockAsync<T>(
        Guid sectionId, Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"EduSync.Enrollment.{sectionId:N}";
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = {resource},
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;
            IF @result < 0 THROW 51001, 'Could not acquire enrollment lock.', 1;
            """, cancellationToken);
        var result = await action();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public Task<EnrollmentSectionInfo?> GetSectionAsync(Guid sectionId, CancellationToken cancellationToken) =>
        context.CourseSections.AsNoTracking()
            .Where(section => section.Id == sectionId)
            .Select(section => new EnrollmentSectionInfo(
                section.Id,
                section.Lecturer.UserId,
                section.Capacity,
                section.Status,
                section.Enrollments.Count))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<EnrollmentStudentInfo?> GetStudentByCodeAsync(string studentCode, CancellationToken cancellationToken) =>
        context.Students.AsNoTracking()
            .Where(student => student.StudentCode == studentCode)
            .Select(student => new EnrollmentStudentInfo(
                student.Id, student.Status, student.User.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid sectionId, Guid studentId, CancellationToken cancellationToken) =>
        context.Enrollments.AnyAsync(enrollment =>
            enrollment.CourseSectionId == sectionId && enrollment.StudentId == studentId,
            cancellationToken);

    public async Task CreateAsync(EnrollmentEntity enrollment, CancellationToken cancellationToken)
    {
        context.Enrollments.Add(new Enrollment
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseSectionId = enrollment.CourseSectionId,
            EnrolledAt = enrollment.EnrolledAt,
            Status = enrollment.Status
        });
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new EnrollmentException(EnrollmentError.AlreadyEnrolled,
                "Student is already enrolled in this course section.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
        {
            throw new EnrollmentException(EnrollmentError.InvalidData,
                "Course section or student no longer exists.");
        }
    }
}
