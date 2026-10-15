using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class SemesterRepository(MyFschoolDemoContext context) : ISemesterRepository
{
    public Task<bool> ExistsAsync(string academicYear, string name, CancellationToken cancellationToken) =>
        context.Semesters.AnyAsync(
            semester => semester.AcademicYear == academicYear && semester.Name == name,
            cancellationToken);

    public async Task CreateAsync(SemesterEntity semester, CancellationToken cancellationToken)
    {
        context.Semesters.Add(semester.ToDatabaseModel());
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new SemesterException(SemesterError.AlreadyExists,
                "Semester already exists for this academic year.");
        }
    }

    public async Task<IReadOnlyList<SemesterEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await context.Semesters
            .AsNoTracking()
            .OrderByDescending(semester => semester.StartDate)
            .ThenBy(semester => semester.Name)
            .ThenBy(semester => semester.Id)
            .ToListAsync(cancellationToken);

        return models.Select(model => model.ToDomainEntity()).ToArray();
    }
}
