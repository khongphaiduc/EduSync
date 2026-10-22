using System.Security.Claims;
using EduSync.API.Common;
using EduSync.API.Contracts.Attendance;
using EduSync.Application.DTOs.Attendance;
using EduSync.Application.Services.Attendance;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Lecturer))]
[Route("api/class-sessions/{sessionId:guid}/attendance")]
public sealed class ClassSessionAttendanceController(IAttendanceService attendances) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<SessionAttendanceDto>>> Get(
        Guid sessionId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(ApiResponse<SessionAttendanceDto>.Failed(
                "Access token has no valid user ID."));
        }

        var result = await attendances.GetAsync(sessionId, userId, cancellationToken);
        return Ok(ApiResponse<SessionAttendanceDto>.Succeeded(result));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<SessionAttendanceDto>>> Mark(
        Guid sessionId, MarkAttendanceRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(ApiResponse<SessionAttendanceDto>.Failed(
                "Access token has no valid user ID."));
        }

        if (request.Items.Any(item => item is null))
        {
            return BadRequest(ApiResponse<SessionAttendanceDto>.Failed(
                "Attendance items are invalid."));
        }

        var items = request.Items.Select(item => new MarkAttendanceItem(
            item.EnrollmentId, item.Status, item.Note)).ToArray();
        var result = await attendances.MarkAsync(
            sessionId, userId, items, cancellationToken);
        return Ok(ApiResponse<SessionAttendanceDto>.Succeeded(result,
            "Attendance saved."));
    }
}
