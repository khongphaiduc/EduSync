using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EduSync.API.Common;
using EduSync.Application.DTOs.Grades;
using EduSync.Application.Services.Grades;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Lecturer")]
[Route("api/course-sections/{sectionId:guid}/grades")]
public sealed class CourseSectionGradesController(ISectionGradeService grades) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<SectionGradeListDto>>> List(
        Guid sectionId,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        Guid? lecturerUserId = null;
        if (!User.IsInRole(nameof(UserRole.Admin)))
        {
            if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
            {
                return Unauthorized(ApiResponse<SectionGradeListDto>.Failed(
                    "Access token has no valid user ID."));
            }

            lecturerUserId = userId;
        }

        var result = await grades.ListAsync(
            sectionId, page, pageSize, lecturerUserId, cancellationToken);
        return Ok(ApiResponse<SectionGradeListDto>.Succeeded(result));
    }
}
