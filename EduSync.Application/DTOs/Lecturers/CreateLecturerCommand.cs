namespace EduSync.Application.DTOs.Lecturers;

public sealed record CreateLecturerCommand(
    string Email,
    string Password,
    string LecturerCode,
    string FullName,
    string? Phone);
