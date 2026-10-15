namespace EduSync.Application.DTOs.Semesters;

public sealed record SemesterDto(
    Guid Id,
    string Name,
    string AcademicYear,
    DateOnly StartDate,
    DateOnly EndDate);
