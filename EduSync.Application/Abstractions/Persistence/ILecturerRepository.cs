using EduSync.Application.DTOs.Lecturers;
using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public interface ILecturerRepository
{
    Task<bool> LecturerCodeExistsAsync(string lecturerCode, CancellationToken cancellationToken);

    Task CreateAsync(UserEntity user, LecturerEntity lecturer, CancellationToken cancellationToken);

    Task<LecturerListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
}
