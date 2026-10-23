namespace EduSync.Application.DTOs.Majors;

public sealed record MajorDto(Guid Id, string MajorCode, string MajorName, string? Description);
