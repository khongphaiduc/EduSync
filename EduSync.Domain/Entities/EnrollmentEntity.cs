namespace EduSync.Domain.Entities;

public class EnrollmentEntity
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid CourseSectionId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
