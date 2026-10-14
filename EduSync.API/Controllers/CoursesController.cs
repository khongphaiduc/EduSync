using System.ComponentModel.DataAnnotations;
using EduSync.API.Common;
using EduSync.Application.DTOs.Courses;
using EduSync.Application.Services.Courses;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(ICourseService courses) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CourseDto>>>> List(
        [FromQuery, Range(1, 100)] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var items = await courses.ListAsync(limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CourseDto>>.Succeeded(items));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var course = await courses.GetByIdAsync(id, cancellationToken);
        return course is null
            ? NotFound(ApiResponse<CourseDto>.Failed("Course not found."))
            : Ok(ApiResponse<CourseDto>.Succeeded(course));
    }
}
