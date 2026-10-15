using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface ICourseRepository
{
    Task<bool> CourseCodeExistsAsync(string courseCode, CancellationToken cancellationToken);

    Task CreateAsync(CourseEntity course, CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseEntity>> ListAsync(int limit, CancellationToken cancellationToken = default);

    Task<CourseEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
