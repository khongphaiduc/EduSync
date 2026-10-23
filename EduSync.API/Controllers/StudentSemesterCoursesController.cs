using System.Security.Claims;
using EduSync.API.Common;
using EduSync.Application.DTOs.CourseSections;
using EduSync.Application.Services.CourseSections;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student))]
[Route("api/students/me/semesters/{semesterId:guid}/courses")]
public sealed class StudentSemesterCoursesController(ICourseSectionService sections) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentSemesterCourseDto>>>> List(
        Guid semesterId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyList<StudentSemesterCourseDto>>.Failed(
                "Access token has no valid user ID."));
        }

        var result = await sections.ListForStudentSemesterAsync(
            userId, semesterId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StudentSemesterCourseDto>>.Succeeded(result));
    }
}
