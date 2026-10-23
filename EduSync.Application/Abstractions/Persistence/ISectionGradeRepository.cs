using EduSync.Application.DTOs.Grades;

namespace EduSync.Application.Abstractions.Persistence;

public interface ISectionGradeRepository
{
    Task<GradeSectionInfo?> GetSectionAsync(Guid sectionId, CancellationToken cancellationToken);

    Task<SectionGradeListDto> ListAsync(
        Guid sectionId, int page, int pageSize, CancellationToken cancellationToken);

    Task<StudentSectionGradesDto?> GetForStudentAsync(
        Guid sectionId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SavedGradeDto>> SaveAsync(
        Guid sectionId, IReadOnlyList<SaveGradeItem> items, CancellationToken cancellationToken);
}
