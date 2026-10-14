namespace EduSync.Domain.Entities;

public class RequestEntity
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public string RequestType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }

    public Guid? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? Response { get; set; }
}
