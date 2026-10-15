using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Semesters;

public sealed class CreateSemesterRequest
{
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(20)]
    public string AcademicYear { get; init; } = string.Empty;

    [Required]
    public DateOnly StartDate { get; init; }

    [Required]
    public DateOnly EndDate { get; init; }
}
