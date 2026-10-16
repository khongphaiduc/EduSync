using EduSync.Application.DTOs.Requests;

namespace EduSync.Application.Services.Requests;

public interface IRequestService
{
    Task<RequestListDto> ListAsync(
        int page,
        int pageSize,
        string? status,
        string? requestType,
        Guid? studentId,
        CancellationToken cancellationToken = default);
}
