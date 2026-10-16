namespace EduSync.Application.DTOs.GradeComponents;

public sealed record CreateGradeComponentCommand(Guid CourseSectionId, string Name, decimal Weight, decimal MaxScore);
