using System.ComponentModel.DataAnnotations;
using EduSync.API.Common;
using EduSync.API.Contracts.Courses;
using EduSync.Application.DTOs.Courses;
using EduSync.Application.Services.Courses;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(ICourseService courses) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ApiResponse<CourseDto>>> Create(
        CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCourseCommand(
            request.CourseCode,
            request.CourseName,
            request.Credits,
            request.Description);
        var result = await courses.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<CourseDto>.Succeeded(result, "Course created."));
    }

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
