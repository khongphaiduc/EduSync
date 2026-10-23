using EduSync.Application.DTOs.Majors;

namespace EduSync.Application.Services.Majors;

public interface IMajorService
{
    Task<IReadOnlyList<MajorDto>> ListAsync(CancellationToken cancellationToken = default);
}
