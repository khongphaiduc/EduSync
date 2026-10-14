using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.DTOs.Courses;

namespace EduSync.Application.Services.Courses;

public sealed class CourseService(ICourseRepository courses) : ICourseService
{
    public async Task<IReadOnlyList<CourseDto>> ListAsync(int limit, CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");
        }

        var entities = await courses.ListAsync(limit, cancellationToken);
        return entities.Select(course => new CourseDto(
            course.Id,
            course.CourseCode,
            course.CourseName,
            course.Credits,
            course.Description)).ToArray();
    }

    public async Task<CourseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await courses.GetByIdAsync(id, cancellationToken);
        return course is null
            ? null
            : new CourseDto(course.Id, course.CourseCode, course.CourseName, course.Credits, course.Description);
    }
}
