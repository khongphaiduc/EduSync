using EduSync.Application.DTOs.CourseSections;

namespace EduSync.Application.Services.CourseSections;

public interface ICourseSectionService
{
    Task<CourseSectionDto> CreateAsync(
        CreateCourseSectionCommand command,
        CancellationToken cancellationToken = default);

    Task<CourseSectionListDto> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CourseSectionListDto> ListForLecturerAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentSemesterCourseDto>> ListForStudentSemesterAsync(
        Guid userId,
        Guid semesterId,
        CancellationToken cancellationToken = default);
}
