namespace EduSync.Application.DTOs.Enrollments;

public sealed record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    Guid CourseSectionId,
    DateTime EnrolledAt,
    string Status);
