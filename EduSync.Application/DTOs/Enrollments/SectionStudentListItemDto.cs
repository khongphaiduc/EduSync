namespace EduSync.Application.DTOs.Enrollments;

public sealed record SectionStudentListItemDto(
    Guid EnrollmentId,
    Guid StudentId,
    string StudentCode,
    string FullName,
    string Email,
    string StudentStatus,
    string EnrollmentStatus,
    DateTime EnrolledAt);
