using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.DTOs.Students;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class StudentRepository(MyFschoolDemoContext context) : IStudentRepository
{
    public Task<StudentListDto> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ListAsync(context.Students.AsNoTracking(), page, pageSize, cancellationToken);

    public Task<StudentListDto> SearchAsync(
        string search,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ListAsync(context.Students.AsNoTracking()
            .Where(student => student.StudentCode.Contains(search)
                || student.FullName.Contains(search)),
            page, pageSize, cancellationToken);

    private static async Task<StudentListDto> ListAsync(
        IQueryable<Student> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new StudentListDto([], page, pageSize, totalCount);
        }

        var items = await query
            .OrderBy(student => student.StudentCode)
            .ThenBy(student => student.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(student => new StudentListItemDto(
                student.Id,
                student.StudentCode,
                student.FullName,
                student.User.Email,
                student.MajorId,
                student.Major.MajorName,
                student.EnrollmentYear,
                student.Status,
                student.AvatarUrl))
            .ToArrayAsync(cancellationToken);

        return new StudentListDto(items, page, pageSize, totalCount);
    }
}
