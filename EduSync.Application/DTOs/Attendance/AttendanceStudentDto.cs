namespace EduSync.Application.DTOs.Attendance;

public sealed record AttendanceStudentDto(
    Guid EnrollmentId,
    Guid StudentId,
    string StudentCode,
    string FullName,
    string EnrollmentStatus,
    string? AttendanceStatus,
    string? Note,
    DateTime? RecordedAtUtc);
