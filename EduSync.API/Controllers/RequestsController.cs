using System.ComponentModel.DataAnnotations;
using EduSync.API.Common;
using EduSync.Application.DTOs.Requests;
using EduSync.Application.Services.Requests;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/requests")]
public sealed class RequestsController(IRequestService requests) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<RequestListDto>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        [FromQuery, StringLength(20)] string? status = null,
        [FromQuery, StringLength(50)] string? requestType = null,
        [FromQuery] Guid? studentId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await requests.ListAsync(
            page, pageSize, status, requestType, studentId, cancellationToken);
        return Ok(ApiResponse<RequestListDto>.Succeeded(result));
    }
}
