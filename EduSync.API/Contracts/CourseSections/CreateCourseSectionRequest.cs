using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.CourseSections;

public sealed class CreateCourseSectionRequest
{
    [Required]
    public Guid CourseId { get; init; }

    [Required]
    public Guid SemesterId { get; init; }

    [Required]
    public Guid LecturerId { get; init; }

    [Required, StringLength(30)]
    public string SectionCode { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; init; }
}
