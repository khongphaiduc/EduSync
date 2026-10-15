using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Courses;

public sealed class CreateCourseRequest
{
    [Required, StringLength(30)]
    public string CourseCode { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string CourseName { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Credits { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }
}
