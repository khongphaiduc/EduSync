using System.ComponentModel.DataAnnotations;
using EduSync.API.Common;
using EduSync.API.Contracts.Lecturers;
using EduSync.Application.DTOs.Lecturers;
using EduSync.Application.Services.Lecturers;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/lecturers")]
public sealed class LecturersController(ILecturerService lecturers) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LecturerCreatedDto>>> Create(
        CreateLecturerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLecturerCommand(
            request.Email,
            request.Password,
            request.LecturerCode,
            request.FullName,
            request.Phone);
        var result = await lecturers.CreateAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<LecturerCreatedDto>.Succeeded(result, "Lecturer created."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<LecturerListDto>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await lecturers.ListAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<LecturerListDto>.Succeeded(result));
    }
}
