using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface ICourseRepository
{
    Task<IReadOnlyList<CourseEntity>> ListAsync(int limit, CancellationToken cancellationToken = default);

    Task<CourseEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
