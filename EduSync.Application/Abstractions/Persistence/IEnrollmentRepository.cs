using EduSync.Domain.Entities;
using EduSync.Application.DTOs.Enrollments;

namespace EduSync.Application.Abstractions.Persistence;

public interface IEnrollmentRepository
{
    Task<T> WithSectionLockAsync<T>(
        Guid sectionId, Func<Task<T>> action, CancellationToken cancellationToken);

    Task<EnrollmentSectionInfo?> GetSectionAsync(Guid sectionId, CancellationToken cancellationToken);

    Task<SectionStudentListDto> ListStudentsAsync(
        Guid sectionId, int page, int pageSize, CancellationToken cancellationToken);

    Task<EnrollmentStudentInfo?> GetStudentAsync(Guid studentId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid sectionId, Guid studentId, CancellationToken cancellationToken);

    Task CreateAsync(EnrollmentEntity enrollment, CancellationToken cancellationToken);
}
