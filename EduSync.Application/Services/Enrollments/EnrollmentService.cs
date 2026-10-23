using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Enrollments;
using EduSync.Domain.Entities;

namespace EduSync.Application.Services.Enrollments;

public sealed class EnrollmentService(IEnrollmentRepository enrollments) : IEnrollmentService
{
    public async Task<SectionStudentListDto> ListStudentsAsync(
        Guid sectionId, int page, int pageSize, Guid? lecturerUserId,
        CancellationToken cancellationToken = default)
    {
        if (sectionId == Guid.Empty)
        {
            throw new EnrollmentException(EnrollmentError.InvalidData,
                "Course section ID is required.");
        }

        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize),
                "Page size must be between 1 and 100.");
        }

        var section = await enrollments.GetSectionAsync(sectionId, cancellationToken);
        if (section is null)
        {
            throw new EnrollmentException(EnrollmentError.SectionNotFound,
                "Course section does not exist.");
        }

        if (lecturerUserId is not null && section.LecturerUserId != lecturerUserId.Value)
        {
            throw new EnrollmentException(EnrollmentError.NotAssigned,
                "Lecturer is not assigned to this course section.");
        }

        return await enrollments.ListStudentsAsync(sectionId, page, pageSize, cancellationToken);
    }

    public Task<EnrollmentDto> AddStudentAsync(
        Guid sectionId, string studentCode, CancellationToken cancellationToken = default)
    {
        studentCode = studentCode?.Trim() ?? string.Empty;
        if (sectionId == Guid.Empty || string.IsNullOrWhiteSpace(studentCode) || studentCode.Length > 30)
        {
            throw new EnrollmentException(EnrollmentError.InvalidData,
                "Course section ID and a valid student code are required.");
        }

        return enrollments.WithSectionLockAsync(sectionId, async () =>
        {
            var section = await enrollments.GetSectionAsync(sectionId, cancellationToken)
                ?? throw new EnrollmentException(EnrollmentError.SectionNotFound,
                    "Course section does not exist.");
            var student = await enrollments.GetStudentByCodeAsync(studentCode, cancellationToken)
                ?? throw new EnrollmentException(EnrollmentError.StudentNotFound,
                    "Student does not exist.");

            if (await enrollments.ExistsAsync(sectionId, student.Id, cancellationToken))
            {
                throw new EnrollmentException(EnrollmentError.AlreadyEnrolled,
                    "Student is already enrolled in this course section.");
            }

            if (section.Status != "Open")
            {
                throw new EnrollmentException(EnrollmentError.SectionClosed,
                    "Course section is not open for enrollment.");
            }

            if (!student.IsActive || student.Status != "Studying")
            {
                throw new EnrollmentException(EnrollmentError.StudentInactive,
                    "Student is not active for enrollment.");
            }

            if (section.EnrollmentCount >= section.Capacity)
            {
                throw new EnrollmentException(EnrollmentError.SectionFull,
                    "Course section is full.");
            }

            var enrollment = new EnrollmentEntity
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                CourseSectionId = sectionId,
                EnrolledAt = DateTime.UtcNow,
                Status = "Registered"
            };
            await enrollments.CreateAsync(enrollment, cancellationToken);
            return new EnrollmentDto(enrollment.Id, enrollment.StudentId,
                enrollment.CourseSectionId, enrollment.EnrolledAt, enrollment.Status);
        }, cancellationToken);
    }
}
