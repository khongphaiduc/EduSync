namespace EduSync.Domain.Entities;

public class GradeComponentEntity
{
    public Guid Id { get; set; }

    public Guid CourseSectionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public decimal MaxScore { get; set; }
}
