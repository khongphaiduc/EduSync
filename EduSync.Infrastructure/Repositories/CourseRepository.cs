using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class CourseRepository(MyFschoolDemoContext context) : ICourseRepository
{
    public Task<bool> CourseCodeExistsAsync(string courseCode, CancellationToken cancellationToken) =>
        context.Courses.AnyAsync(course => course.CourseCode == courseCode, cancellationToken);

    public async Task CreateAsync(CourseEntity course, CancellationToken cancellationToken)
    {
        context.Courses.Add(course.ToDatabaseModel());
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new CourseException(CourseError.CodeAlreadyExists, "Course code already exists.");
        }
    }

    public async Task<IReadOnlyList<CourseEntity>> ListAsync(int limit, CancellationToken cancellationToken = default)
    {
        var models = await context.Courses
            .AsNoTracking()
            .OrderBy(course => course.CourseCode)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return models.Select(model => model.ToDomainEntity()).ToArray();
    }

    public async Task<CourseEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await context.Courses
            .AsNoTracking()
            .SingleOrDefaultAsync(course => course.Id == id, cancellationToken);

        return model?.ToDomainEntity();
    }
}
