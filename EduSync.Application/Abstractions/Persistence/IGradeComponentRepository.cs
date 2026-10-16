using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface IGradeComponentRepository
{
    Task<T> WithSectionLockAsync<T>(Guid sectionId, Func<Task<T>> action, CancellationToken cancellationToken);

    Task<bool> SectionExistsAsync(Guid sectionId, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(Guid sectionId, string name, CancellationToken cancellationToken);

    Task<decimal> GetTotalWeightAsync(Guid sectionId, CancellationToken cancellationToken);

    Task CreateAsync(GradeComponentEntity component, CancellationToken cancellationToken);
}
