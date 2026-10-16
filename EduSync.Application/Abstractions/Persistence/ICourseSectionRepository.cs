using EduSync.Domain.Entities;
using EduSync.Application.DTOs.CourseSections;

namespace EduSync.Application.Abstractions.Persistence;

public interface ICourseSectionRepository
{
    Task<bool> CourseExistsAsync(Guid courseId, CancellationToken cancellationToken);

    Task<bool> SemesterExistsAsync(Guid semesterId, CancellationToken cancellationToken);

    Task<bool> LecturerExistsAsync(Guid lecturerId, CancellationToken cancellationToken);

    Task<bool> SectionCodeExistsAsync(Guid semesterId, string sectionCode, CancellationToken cancellationToken);

    Task CreateAsync(CourseSectionEntity section, CancellationToken cancellationToken);

    Task<CourseSectionListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<CourseSectionListDto> ListForLecturerAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}
