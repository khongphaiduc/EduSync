namespace EduSync.Application.DTOs.CourseSections;

public sealed record CreateCourseSectionCommand(
    Guid CourseId,
    Guid SemesterId,
    Guid LecturerId,
    string SectionCode,
    int Capacity);
