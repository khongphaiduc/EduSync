namespace EduSync.Domain.Entities;

public class ClubMemberEntity
{
    public Guid Id { get; set; }

    public Guid ClubId { get; set; }

    public Guid StudentId { get; set; }

    public DateTime JoinedAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
