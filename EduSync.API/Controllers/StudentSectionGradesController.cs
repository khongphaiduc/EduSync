using System.Security.Claims;
using EduSync.API.Common;
using EduSync.Application.DTOs.Grades;
using EduSync.Application.Services.Grades;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student))]
[Route("api/students/me/course-sections/{sectionId:guid}/grades")]
public sealed class StudentSectionGradesController(ISectionGradeService grades) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<StudentSectionGradesDto>>> Get(
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(ApiResponse<StudentSectionGradesDto>.Failed(
                "Access token has no valid user ID."));
        }

        var result = await grades.GetForStudentAsync(sectionId, userId, cancellationToken);
        return Ok(ApiResponse<StudentSectionGradesDto>.Succeeded(result));
    }
}
