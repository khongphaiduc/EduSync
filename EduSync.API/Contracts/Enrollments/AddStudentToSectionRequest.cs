using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Enrollments;

public sealed class AddStudentToSectionRequest
{
    [Required]
    public Guid StudentId { get; init; }
}
