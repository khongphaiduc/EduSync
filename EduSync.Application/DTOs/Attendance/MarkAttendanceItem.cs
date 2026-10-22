namespace EduSync.Application.DTOs.Attendance;

public sealed record MarkAttendanceItem(
    Guid EnrollmentId,
    string Status,
    string? Note);
