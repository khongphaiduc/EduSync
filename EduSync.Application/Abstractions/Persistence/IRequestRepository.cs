using EduSync.Application.DTOs.Requests;

namespace EduSync.Application.Abstractions.Persistence;

public interface IRequestRepository
{
    Task<RequestListDto> ListAsync(
        int page,
        int pageSize,
        string? status,
        string? requestType,
        Guid? studentId,
        CancellationToken cancellationToken = default);
}
