using EduSync.Application.DTOs.Students;

namespace EduSync.Application.Services.Students;

public interface IStudentService
{
    Task<StudentCreatedDto> CreateAsync(CreateStudentCommand command, CancellationToken cancellationToken = default);

    Task<StudentListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<StudentListDto> SearchAsync(
        string search, int page, int pageSize, CancellationToken cancellationToken = default);
}
