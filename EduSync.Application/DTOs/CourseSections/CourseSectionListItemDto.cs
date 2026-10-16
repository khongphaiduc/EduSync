namespace EduSync.Application.DTOs.CourseSections;

public sealed record CourseSectionListItemDto(
    Guid Id,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    Guid SemesterId,
    string SemesterName,
    string AcademicYear,
    Guid LecturerId,
    string LecturerName,
    string SectionCode,
    int Capacity,
    string Status);
