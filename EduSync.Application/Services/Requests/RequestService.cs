using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.DTOs.Requests;

namespace EduSync.Application.Services.Requests;

public sealed class RequestService(IRequestRepository requests) : IRequestService
{
    public Task<RequestListDto> ListAsync(
        int page,
        int pageSize,
        string? status,
        string? requestType,
        Guid? studentId,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        status = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
        requestType = string.IsNullOrWhiteSpace(requestType) ? null : requestType.Trim();
        if (status?.Length > 20 || requestType?.Length > 50)
        {
            throw new ArgumentException("Request filters are invalid.");
        }

        return requests.ListAsync(page, pageSize, status, requestType, studentId, cancellationToken);
    }
}
