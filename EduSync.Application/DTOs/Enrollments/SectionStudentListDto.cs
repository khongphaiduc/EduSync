namespace EduSync.Application.DTOs.Enrollments;

public sealed record SectionStudentListDto(
    IReadOnlyList<SectionStudentListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
