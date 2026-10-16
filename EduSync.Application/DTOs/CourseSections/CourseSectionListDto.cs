namespace EduSync.Application.DTOs.CourseSections;

public sealed record CourseSectionListDto(
    IReadOnlyList<CourseSectionListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
