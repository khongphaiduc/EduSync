namespace EduSync.Application.DTOs.Grades;

public sealed record SavedGradeDto(
    Guid EnrollmentId,
    Guid GradeComponentId,
    Guid? GradeId,
    decimal? Score,
    DateTime? GradedAtUtc,
    DateTime? UpdatedAtUtc);
