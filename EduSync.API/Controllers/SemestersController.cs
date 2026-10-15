using EduSync.API.Common;
using EduSync.API.Contracts.Semesters;
using EduSync.Application.DTOs.Semesters;
using EduSync.Application.Services.Semesters;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Route("api/semesters")]
public sealed class SemestersController(ISemesterService semesters) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ApiResponse<SemesterDto>>> Create(
        CreateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSemesterCommand(
            request.Name,
            request.AcademicYear,
            request.StartDate,
            request.EndDate);
        var result = await semesters.CreateAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<SemesterDto>.Succeeded(result, "Semester created."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SemesterDto>>>> List(
        CancellationToken cancellationToken)
    {
        var items = await semesters.ListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SemesterDto>>.Succeeded(items));
    }
}
