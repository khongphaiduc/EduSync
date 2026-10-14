using EduSync.Application.DTOs.Courses;

namespace EduSync.Application.Services.Courses;

public interface ICourseService
{
    Task<IReadOnlyList<CourseDto>> ListAsync(int limit, CancellationToken cancellationToken = default);

    Task<CourseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
