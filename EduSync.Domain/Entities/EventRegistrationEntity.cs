namespace EduSync.Domain.Entities;

public class EventRegistrationEntity
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Guid StudentId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
