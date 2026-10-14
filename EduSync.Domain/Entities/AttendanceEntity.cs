namespace EduSync.Domain.Entities;

public class AttendanceEntity
{
    public Guid Id { get; set; }

    public Guid EnrollmentId { get; set; }

    public Guid ClassSessionId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }

    public DateTime RecordedAt { get; set; }
}
