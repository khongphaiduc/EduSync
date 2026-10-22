using EduSync.Application.DTOs.Grades;

namespace EduSync.Application.Services.Grades;

public interface ISectionGradeService
{
    Task<SectionGradeListDto> ListAsync(
        Guid sectionId,
        int page,
        int pageSize,
        Guid? lecturerUserId,
        CancellationToken cancellationToken = default);
}
