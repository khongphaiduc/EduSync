using EduSync.Application.DTOs.Students;

namespace EduSync.Application.Abstractions.Persistence;

public interface IStudentRepository
{
    Task<StudentListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
}
