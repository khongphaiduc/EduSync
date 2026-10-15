namespace EduSync.Application.DTOs.Courses;

public sealed record CreateCourseCommand(
    string CourseCode,
    string CourseName,
    int Credits,
    string? Description);
