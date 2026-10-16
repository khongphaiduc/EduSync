namespace EduSync.Application.DTOs.CourseSections;

public sealed record CourseSectionDto(
    Guid Id,
    Guid CourseId,
    Guid SemesterId,
    Guid LecturerId,
    string SectionCode,
    int Capacity,
    string Status);
