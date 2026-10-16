using EduSync.API.Common;
using EduSync.API.Contracts.GradeComponents;
using EduSync.Application.DTOs.GradeComponents;
using EduSync.Application.Services.GradeComponents;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/course-sections/{sectionId:guid}/grade-components")]
public sealed class GradeComponentsController(IGradeComponentService components) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<GradeComponentDto>>> Create(
        Guid sectionId, CreateGradeComponentRequest request, CancellationToken cancellationToken)
    {
        var result = await components.CreateAsync(new CreateGradeComponentCommand(
            sectionId, request.Name, request.Weight, request.MaxScore), cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<GradeComponentDto>.Succeeded(result, "Grade component created."));
    }
}
