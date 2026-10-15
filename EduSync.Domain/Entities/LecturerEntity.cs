namespace EduSync.Domain.Entities;

public class LecturerEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string LecturerCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Phone { get; set; }

}
