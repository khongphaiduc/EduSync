namespace EduSync.Application.DTOs.Requests;

public sealed record RequestListItemDto(
    Guid Id,
    Guid StudentId,
    string StudentCode,
    string StudentName,
    string RequestType,
    string Title,
    string Content,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string Status,
    DateTime SubmittedAt,
    Guid? ReviewedBy,
    DateTime? ReviewedAt,
    string? Response);
