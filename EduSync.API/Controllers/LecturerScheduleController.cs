using System.Security.Claims;
using EduSync.API.Common;
using EduSync.Application.DTOs.ClassSessions;
using EduSync.Application.Services.ClassSessions;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Lecturer))]
[Route("api/lecturers/me/schedule")]
public sealed class LecturerScheduleController(IClassSessionService sessions) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LecturerScheduleItemDto>>>> List(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] string timeZoneId = "Asia/Ho_Chi_Minh",
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyList<LecturerScheduleItemDto>>.Failed(
                "Access token has no valid user ID."));
        }

        var result = await sessions.ListForCurrentLecturerAsync(
            userId, from, to, timeZoneId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<LecturerScheduleItemDto>>.Succeeded(result));
    }
}
