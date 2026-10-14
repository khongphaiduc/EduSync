namespace EduSync.Domain.Entities;

public class ClubEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? ContactInfo { get; set; }

    public bool IsActive { get; set; }
}
