using EduSync.Application.DTOs.Lecturers;

namespace EduSync.Application.Services.Lecturers;

public interface ILecturerService
{
    Task<LecturerCreatedDto> CreateAsync(CreateLecturerCommand command, CancellationToken cancellationToken = default);

    Task<LecturerListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
