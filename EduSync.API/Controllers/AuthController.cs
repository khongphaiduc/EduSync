using EduSync.API.Common;
using EduSync.API.Contracts.Auth;
using EduSync.Application.DTOs.Auth;
using EduSync.Application.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduSync.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthTokensDto>>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterStudentCommand(
            request.Email,
            request.Password,
            request.StudentCode,
            request.FullName,
            request.MajorId,
            request.EnrollmentYear,
            request.DateOfBirth,
            request.Gender,
            request.Phone);
        var tokens = await auth.RegisterAsync(
            command,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        return Ok(ApiResponse<AuthTokensDto>.Succeeded(tokens, "Registered."));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthTokensDto>>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await auth.LoginAsync(
            request.Email,
            request.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        return Ok(ApiResponse<AuthTokensDto>.Succeeded(tokens, "Logged in."));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthTokensDto>>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await auth.RefreshAsync(
            request.RefreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        return Ok(ApiResponse<AuthTokensDto>.Succeeded(tokens, "Token refreshed."));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object?>>> Logout(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await auth.LogoutAsync(request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<object?>.Succeeded(null, "Logged out."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<AuthUserDto>>> Me(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
        {
            return Unauthorized(ApiResponse<AuthUserDto>.Failed("Invalid access token."));
        }

        var user = await auth.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<AuthUserDto>.Succeeded(user));
    }
}
