using EduSync.Application.DTOs.Courses;

namespace EduSync.Application.Services.Courses;

public interface ICourseService
{
    Task<CourseDto> CreateAsync(CreateCourseCommand command, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseDto>> ListAsync(int limit, CancellationToken cancellationToken = default);

    Task<CourseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
