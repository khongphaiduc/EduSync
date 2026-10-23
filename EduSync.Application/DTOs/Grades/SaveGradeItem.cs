namespace EduSync.Application.DTOs.Grades;

public sealed record SaveGradeItem(Guid EnrollmentId, Guid GradeComponentId, decimal? Score);
