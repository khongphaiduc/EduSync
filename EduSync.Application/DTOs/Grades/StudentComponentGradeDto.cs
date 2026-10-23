namespace EduSync.Application.DTOs.Grades;

public sealed record StudentComponentGradeDto(
    Guid GradeComponentId,
    string Name,
    decimal Weight,
    decimal MaxScore,
    Guid? GradeId,
    decimal? Score,
    DateTime? GradedAtUtc,
    DateTime? UpdatedAtUtc);
