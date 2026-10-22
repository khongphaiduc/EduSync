namespace EduSync.Application.DTOs.Attendance;

public sealed record SessionAttendanceDto(
    Guid ClassSessionId,
    Guid CourseSectionId,
    IReadOnlyList<AttendanceStudentDto> Students);
