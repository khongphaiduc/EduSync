namespace EduSync.Application.DTOs.Requests;

public sealed record RequestListDto(
    IReadOnlyList<RequestListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
