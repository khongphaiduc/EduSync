using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Students;

public sealed class CreateStudentRequest
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required, StringLength(30)]
    public string StudentCode { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    public Guid MajorId { get; init; }

    [Range(1900, 2100)]
    public int EnrollmentYear { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [StringLength(10)]
    public string? Gender { get; init; }

    [StringLength(20)]
    public string? Phone { get; init; }
}
