using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Enrollments;

public sealed class AddStudentToSectionRequest
{
    [Required]
    [StringLength(30)]
    public string StudentCode { get; init; } = string.Empty;
}
