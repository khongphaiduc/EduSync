using EduSync.Application.DTOs.Students;

namespace EduSync.Application.Abstractions.Persistence;

public interface IStudentRepository
{
    Task<StudentListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<StudentListDto> SearchAsync(
        string search, int page, int pageSize, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Student search is not supported by this repository.");
}
