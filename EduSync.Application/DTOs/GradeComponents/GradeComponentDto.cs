namespace EduSync.Application.DTOs.GradeComponents;

public sealed record GradeComponentDto(Guid Id, Guid CourseSectionId, string Name, decimal Weight, decimal MaxScore);
