namespace EduSync.Application.DTOs.Students;

public sealed record StudentListDto(
    IReadOnlyList<StudentListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
