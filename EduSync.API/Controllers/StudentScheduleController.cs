using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EduSync.API.Common;
using EduSync.Application.DTOs.ClassSessions;
using EduSync.Application.Services.ClassSessions;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student))]
[Route("api/students/me/schedule")]
public sealed class StudentScheduleController(IClassSessionService sessions) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentScheduleItemDto>>>> List(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] string timeZoneId = "Asia/Ho_Chi_Minh",
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyList<StudentScheduleItemDto>>.Failed(
                "Access token has no valid user ID."));
        }

        var result = await sessions.ListForCurrentStudentAsync(
            userId, from, to, timeZoneId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StudentScheduleItemDto>>.Succeeded(result));
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentScheduleItemDto>>>> Upcoming(
        [FromQuery, Range(1, 100)] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyList<StudentScheduleItemDto>>.Failed(
                "Access token has no valid user ID."));
        }

        var result = await sessions.ListUpcomingForCurrentStudentAsync(
            userId, limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StudentScheduleItemDto>>.Succeeded(result));
    }
}
