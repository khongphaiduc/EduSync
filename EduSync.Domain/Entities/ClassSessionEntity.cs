namespace EduSync.Domain.Entities;

public class ClassSessionEntity
{
    public Guid Id { get; set; }

    public Guid CourseSectionId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string? Room { get; set; }

    public string? Topic { get; set; }

    public string Status { get; set; } = string.Empty;
}
