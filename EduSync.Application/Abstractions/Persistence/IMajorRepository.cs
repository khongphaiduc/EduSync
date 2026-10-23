using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface IMajorRepository
{
    Task<IReadOnlyList<MajorEntity>> ListAsync(CancellationToken cancellationToken = default);
}
