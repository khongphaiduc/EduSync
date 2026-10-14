namespace EduSync.Application.DTOs.Courses;

public sealed record CourseDto(
    Guid Id,
    string CourseCode,
    string CourseName,
    int Credits,
    string? Description);
