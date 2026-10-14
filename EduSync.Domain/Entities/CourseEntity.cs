namespace EduSync.Domain.Entities;

public class CourseEntity
{
    public Guid Id { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public int Credits { get; set; }

    public string? Description { get; set; }
}
