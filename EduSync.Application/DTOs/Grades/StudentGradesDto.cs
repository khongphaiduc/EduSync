namespace EduSync.Application.DTOs.Grades;

public sealed record StudentGradesDto(
    Guid EnrollmentId,
    Guid StudentId,
    string StudentCode,
    string FullName,
    string EnrollmentStatus,
    IReadOnlyList<StudentGradeDto> Grades);
