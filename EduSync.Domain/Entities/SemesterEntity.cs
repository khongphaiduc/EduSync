namespace EduSync.Domain.Entities;

public class SemesterEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}
