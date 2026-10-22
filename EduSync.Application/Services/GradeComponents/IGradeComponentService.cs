using EduSync.Application.DTOs.GradeComponents;

namespace EduSync.Application.Services.GradeComponents;

public interface IGradeComponentService
{
    Task<IReadOnlyList<GradeComponentDto>> ListAsync(
        Guid sectionId, CancellationToken cancellationToken = default);

    Task<GradeComponentDto> CreateAsync(CreateGradeComponentCommand command, CancellationToken cancellationToken = default);
}
