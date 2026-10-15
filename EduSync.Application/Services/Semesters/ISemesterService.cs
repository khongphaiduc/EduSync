using EduSync.Application.DTOs.Semesters;

namespace EduSync.Application.Services.Semesters;

public interface ISemesterService
{
    Task<SemesterDto> CreateAsync(CreateSemesterCommand command, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SemesterDto>> ListAsync(CancellationToken cancellationToken = default);
}
