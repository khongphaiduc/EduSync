using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.DTOs.Majors;

namespace EduSync.Application.Services.Majors;

public sealed class MajorService(IMajorRepository majors) : IMajorService
{
    public async Task<IReadOnlyList<MajorDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await majors.ListAsync(cancellationToken);
        return entities.Select(major => new MajorDto(
            major.Id,
            major.MajorCode,
            major.MajorName,
            major.Description)).ToArray();
    }
}
