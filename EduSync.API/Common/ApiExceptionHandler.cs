using EduSync.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace EduSync.API.Common;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is SemesterException semesterException)
        {
            httpContext.Response.StatusCode = semesterException.Error == SemesterError.AlreadyExists
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(semesterException.Message),
                cancellationToken);
            return true;
        }

        if (exception is CourseException courseException)
        {
            httpContext.Response.StatusCode = courseException.Error == CourseError.CodeAlreadyExists
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(courseException.Message),
                cancellationToken);
            return true;
        }

        if (exception is AuthException authException)
        {
            httpContext.Response.StatusCode = authException.Error switch
            {
                AuthError.InvalidCredentials or AuthError.InvalidRefreshToken => StatusCodes.Status401Unauthorized,
                AuthError.InactiveAccount => StatusCodes.Status403Forbidden,
                AuthError.EmailAlreadyExists or AuthError.StudentCodeAlreadyExists
                    or AuthError.LecturerCodeAlreadyExists
                    or AuthError.RegistrationConflict => StatusCodes.Status409Conflict,
                AuthError.InvalidMajor or AuthError.InvalidRegistration => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(authException.Message),
                cancellationToken);
            return true;
        }

        logger.LogError(exception, "Unhandled error for {Path}", httpContext.Request.Path);
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse<object?>.Failed("An unexpected error occurred."),
            cancellationToken);

        return true;
    }
}
