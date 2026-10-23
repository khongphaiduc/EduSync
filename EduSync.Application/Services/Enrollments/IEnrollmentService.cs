using EduSync.Application.DTOs.Enrollments;

namespace EduSync.Application.Services.Enrollments;

public interface IEnrollmentService
{
    Task<EnrollmentDto> AddStudentAsync(
        Guid sectionId, string studentCode, CancellationToken cancellationToken = default);

    Task<SectionStudentListDto> ListStudentsAsync(
        Guid sectionId, int page, int pageSize, Guid? lecturerUserId,
        CancellationToken cancellationToken = default);
}
