using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.DTOs.Requests;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class RequestRepository(MyFschoolDemoContext context) : IRequestRepository
{
    public async Task<RequestListDto> ListAsync(
        int page,
        int pageSize,
        string? status,
        string? requestType,
        Guid? studentId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Requests.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(request => request.Status == status);
        }

        if (requestType is not null)
        {
            query = query.Where(request => request.RequestType == requestType);
        }

        if (studentId is not null)
        {
            query = query.Where(request => request.StudentId == studentId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new RequestListDto([], page, pageSize, totalCount);
        }

        var items = await query
            .OrderByDescending(request => request.SubmittedAt)
            .ThenByDescending(request => request.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(request => new RequestListItemDto(
                request.Id,
                request.StudentId,
                request.Student.StudentCode,
                request.Student.FullName,
                request.RequestType,
                request.Title,
                request.Content,
                request.StartDate,
                request.EndDate,
                request.Status,
                request.SubmittedAt,
                request.ReviewedBy,
                request.ReviewedAt,
                request.Response))
            .ToArrayAsync(cancellationToken);

        return new RequestListDto(items, page, pageSize, totalCount);
    }
}
