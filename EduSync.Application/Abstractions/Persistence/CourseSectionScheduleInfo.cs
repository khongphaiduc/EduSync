namespace EduSync.Application.Abstractions.Persistence;

public sealed record CourseSectionScheduleInfo(
    Guid Id,
    Guid LecturerId,
    DateOnly SemesterStartDate,
    DateOnly SemesterEndDate);
