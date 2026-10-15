namespace EduSync.Application.DTOs.Lecturers;

public sealed record LecturerListDto(
    IReadOnlyList<LecturerListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
