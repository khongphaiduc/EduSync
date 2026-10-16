using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EduSync.API.Common;
using EduSync.Application.DTOs.CourseSections;
using EduSync.Application.Services.CourseSections;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Lecturer))]
[Route("api/lecturers/me/course-sections")]
public sealed class LecturerCourseSectionsController(ICourseSectionService sections) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CourseSectionListDto>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(ApiResponse<CourseSectionListDto>.Failed("Access token has no valid user ID."));
        }

        var result = await sections.ListForLecturerAsync(userId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<CourseSectionListDto>.Succeeded(result));
    }
}
