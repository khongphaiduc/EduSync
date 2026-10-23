namespace EduSync.Application.DTOs.Grades;

public sealed record StudentSectionGradesDto(
    Guid CourseSectionId,
    string SectionCode,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    Guid EnrollmentId,
    IReadOnlyList<StudentComponentGradeDto> Components);
