namespace EduSync.Domain.Entities;

public class GradeEntity
{
    public Guid Id { get; set; }

    public Guid EnrollmentId { get; set; }

    public Guid GradeComponentId { get; set; }

    public decimal? Score { get; set; }

    public DateTime? GradedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
