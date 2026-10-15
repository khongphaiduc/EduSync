namespace EduSync.Application.DTOs.Semesters;

public sealed record CreateSemesterCommand(
    string Name,
    string AcademicYear,
    DateOnly StartDate,
    DateOnly EndDate);
