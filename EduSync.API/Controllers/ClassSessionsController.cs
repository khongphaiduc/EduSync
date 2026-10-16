using EduSync.API.Common;
using EduSync.API.Contracts.ClassSessions;
using EduSync.Application.DTOs.ClassSessions;
using EduSync.Application.Services.ClassSessions;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/class-sessions")]
public sealed class ClassSessionsController(IClassSessionService sessions) : ControllerBase
{
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClassSessionDto>>> Update(
        Guid id,
        UpdateClassSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateClassSessionCommand(
            id,
            request.StartAtUtc,
            request.EndAtUtc,
            request.Room,
            request.ClearRoom,
            request.Topic,
            request.ClearTopic,
            request.Status,
            request.TimeZoneId);
        var result = await sessions.UpdateAsync(command, cancellationToken);
        return Ok(ApiResponse<ClassSessionDto>.Succeeded(result, "Class session updated."));
    }
}
