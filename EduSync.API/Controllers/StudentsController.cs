using System.ComponentModel.DataAnnotations;
using EduSync.API.Common;
using EduSync.API.Contracts.Students;
using EduSync.Application.DTOs.Students;
using EduSync.Application.Services.Students;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/students")]
public sealed class StudentsController(IStudentService students) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentCreatedDto>>> Create(
        CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateStudentCommand(
            request.Email,
            request.Password,
            request.StudentCode,
            request.FullName,
            request.MajorId,
            request.EnrollmentYear,
            request.DateOfBirth,
            request.Gender,
            request.Phone);
        var result = await students.CreateAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<StudentCreatedDto>.Succeeded(result, "Student created."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<StudentListDto>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        [FromQuery, StringLength(150)] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = string.IsNullOrWhiteSpace(search)
            ? await students.ListAsync(page, pageSize, cancellationToken)
            : await students.SearchAsync(search, page, pageSize, cancellationToken);
        return Ok(ApiResponse<StudentListDto>.Succeeded(result));
    }
}
