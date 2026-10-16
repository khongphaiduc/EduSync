using EduSync.Application.DTOs.GradeComponents;

namespace EduSync.Application.Services.GradeComponents;

public interface IGradeComponentService
{
    Task<GradeComponentDto> CreateAsync(CreateGradeComponentCommand command, CancellationToken cancellationToken = default);
}
