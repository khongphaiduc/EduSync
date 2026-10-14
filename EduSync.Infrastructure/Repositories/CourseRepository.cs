using EduSync.Application.Abstractions.Persistence;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class CourseRepository(MyFschoolDemoContext context) : ICourseRepository
{
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
