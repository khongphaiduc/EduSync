using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Courses;
using EduSync.Domain.Entities;

namespace EduSync.Application.Services.Courses;

public sealed class CourseService(ICourseRepository courses) : ICourseService
{
    public async Task<CourseDto> CreateAsync(
        CreateCourseCommand command,
        CancellationToken cancellationToken = default)
    {
        var courseCode = command.CourseCode.Trim();
        var courseName = command.CourseName.Trim();
        var description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        if (string.IsNullOrWhiteSpace(courseCode) || courseCode.Length > 30
            || string.IsNullOrWhiteSpace(courseName) || courseName.Length > 200
            || command.Credits < 1 || description?.Length > 1000)
        {
            throw new CourseException(CourseError.InvalidData, "Course data is invalid.");
        }

        if (await courses.CourseCodeExistsAsync(courseCode, cancellationToken))
        {
            throw new CourseException(CourseError.CodeAlreadyExists, "Course code already exists.");
        }

        var course = new CourseEntity
        {
            Id = Guid.NewGuid(),
            CourseCode = courseCode,
            CourseName = courseName,
            Credits = command.Credits,
            Description = description
        };
        await courses.CreateAsync(course, cancellationToken);
        return new CourseDto(course.Id, course.CourseCode, course.CourseName, course.Credits, course.Description);
    }

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
