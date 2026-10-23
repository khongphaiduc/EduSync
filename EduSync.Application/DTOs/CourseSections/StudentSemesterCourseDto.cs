namespace EduSync.Application.DTOs.CourseSections;

public sealed record StudentSemesterCourseDto(
    Guid EnrollmentId,
    string EnrollmentStatus,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    int Credits,
    Guid CourseSectionId,
    string SectionCode,
    string SectionStatus,
    Guid SemesterId,
    string SemesterName,
    string AcademicYear,
    Guid LecturerId,
    string LecturerName);
