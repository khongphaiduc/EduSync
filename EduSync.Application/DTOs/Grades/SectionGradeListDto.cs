using EduSync.Application.DTOs.GradeComponents;

namespace EduSync.Application.DTOs.Grades;

public sealed record SectionGradeListDto(
    Guid CourseSectionId,
    IReadOnlyList<GradeComponentDto> Components,
    IReadOnlyList<StudentGradesDto> Students,
    int Page,
    int PageSize,
    int TotalCount);
