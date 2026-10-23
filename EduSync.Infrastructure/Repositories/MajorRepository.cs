using EduSync.Application.Abstractions.Persistence;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class MajorRepository(MyFschoolDemoContext context) : IMajorRepository
{
    public async Task<IReadOnlyList<MajorEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await context.Majors
            .AsNoTracking()
            .OrderBy(major => major.MajorName)
            .ThenBy(major => major.MajorCode)
            .ThenBy(major => major.Id)
            .ToListAsync(cancellationToken);

        return models.Select(model => model.ToDomainEntity()).ToArray();
    }
}
