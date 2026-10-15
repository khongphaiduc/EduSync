using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface ISemesterRepository
{
    Task<bool> ExistsAsync(string academicYear, string name, CancellationToken cancellationToken);

    Task CreateAsync(SemesterEntity semester, CancellationToken cancellationToken);

    Task<IReadOnlyList<SemesterEntity>> ListAsync(CancellationToken cancellationToken = default);
}
