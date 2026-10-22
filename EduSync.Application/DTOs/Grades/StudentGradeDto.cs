namespace EduSync.Application.DTOs.Grades;

public sealed record StudentGradeDto(
    Guid GradeComponentId,
    Guid? GradeId,
    decimal? Score,
    DateTime? GradedAtUtc,
    DateTime? UpdatedAtUtc);
