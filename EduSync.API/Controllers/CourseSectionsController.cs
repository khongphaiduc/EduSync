using System.ComponentModel.DataAnnotations;
using EduSync.API.Common;
using EduSync.API.Contracts.ClassSessions;
using EduSync.API.Contracts.CourseSections;
using EduSync.API.Contracts.Enrollments;
using EduSync.Application.DTOs.ClassSessions;
using EduSync.Application.DTOs.CourseSections;
using EduSync.Application.DTOs.Enrollments;
using EduSync.Application.Services.ClassSessions;
using EduSync.Application.Services.CourseSections;
using EduSync.Application.Services.Enrollments;
using EduSync.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/course-sections")]
public sealed class CourseSectionsController(
    ICourseSectionService sections,
    IClassSessionService sessions,
    IEnrollmentService enrollments) : ControllerBase
{
    [HttpPost("{sectionId:guid}/students")]
    public async Task<ActionResult<ApiResponse<EnrollmentDto>>> AddStudent(
        Guid sectionId,
        AddStudentToSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await enrollments.AddStudentAsync(
            sectionId, request.StudentId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<EnrollmentDto>.Succeeded(result, "Student enrolled."));
    }

    [HttpGet("{sectionId:guid}/sessions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClassSessionDto>>>> ListSessions(
        Guid sectionId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] string timeZoneId = "Asia/Ho_Chi_Minh",
        CancellationToken cancellationToken = default)
    {
        var result = await sessions.ListAsync(sectionId, from, to, timeZoneId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ClassSessionDto>>.Succeeded(result));
    }

    [HttpPost("{sectionId:guid}/sessions/generate")]
    public async Task<ActionResult<ApiResponse<GeneratedClassSessionsDto>>> GenerateSessions(
        Guid sectionId,
        GenerateClassSessionsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.WeeklySlots.Any(slot => slot is null))
        {
            return BadRequest(ApiResponse<GeneratedClassSessionsDto>.Failed("Weekly slot data is invalid."));
        }

        var command = new GenerateClassSessionsCommand(
            sectionId,
            request.FromDate,
            request.ToDate,
            request.TimeZoneId,
            request.WeeklySlots.Select(slot => new WeeklyScheduleSlot(
                slot.DayOfWeek, slot.StartTime, slot.EndTime, slot.Room)).ToArray(),
            request.ExcludeDates);
        var result = await sessions.GenerateAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<GeneratedClassSessionsDto>.Succeeded(result, "Class sessions created."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CourseSectionListDto>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sections.ListAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<CourseSectionListDto>.Succeeded(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourseSectionDto>>> Create(
        CreateCourseSectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCourseSectionCommand(
            request.CourseId,
            request.SemesterId,
            request.LecturerId,
            request.SectionCode,
            request.Capacity);
        var result = await sections.CreateAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CourseSectionDto>.Succeeded(result, "Course section created."));
    }
}
