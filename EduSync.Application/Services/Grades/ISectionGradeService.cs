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

    Task<StudentSectionGradesDto> GetForStudentAsync(
        Guid sectionId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SavedGradeDto>> SaveAsync(
        Guid sectionId,
        Guid? lecturerUserId,
        IReadOnlyList<SaveGradeItem> items,
        CancellationToken cancellationToken = default);
}
