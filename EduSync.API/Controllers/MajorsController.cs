using EduSync.API.Common;
using EduSync.Application.DTOs.Majors;
using EduSync.Application.Services.Majors;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.API.Controllers;

[ApiController]
[Route("api/majors")]
public sealed class MajorsController(IMajorService majors) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MajorDto>>>> List(
        CancellationToken cancellationToken)
    {
        var items = await majors.ListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MajorDto>>.Succeeded(items));
    }
}
