namespace EduSync.Domain.Entities;

public class CourseSectionEntity
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public Guid SemesterId { get; set; }

    public Guid LecturerId { get; set; }

    public string SectionCode { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Status { get; set; } = string.Empty;
}
