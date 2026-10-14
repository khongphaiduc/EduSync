namespace EduSync.Domain.Entities;

public class StudentEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string StudentCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public Guid MajorId { get; set; }

    public int EnrollmentYear { get; set; }

    public string Status { get; set; } = string.Empty;
}
