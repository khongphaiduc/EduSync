using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Attendance;

public sealed class MarkAttendanceItemRequest
{
    public Guid EnrollmentId { get; init; }

    [Required]
    public string Status { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Note { get; init; }
}
