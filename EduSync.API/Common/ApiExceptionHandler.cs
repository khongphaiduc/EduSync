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
        if (exception is AttendanceException attendanceException)
        {
            httpContext.Response.StatusCode = attendanceException.Error switch
            {
                AttendanceError.SessionNotFound => StatusCodes.Status404NotFound,
                AttendanceError.NotAssigned => StatusCodes.Status403Forbidden,
                AttendanceError.SessionCancelled => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(attendanceException.Message), cancellationToken);
            return true;
        }

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

        if (exception is CourseSectionException sectionException)
        {
            httpContext.Response.StatusCode = sectionException.Error == CourseSectionError.AlreadyExists
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(sectionException.Message),
                cancellationToken);
            return true;
        }

        if (exception is ClassSessionException sessionException)
        {
            httpContext.Response.StatusCode = sessionException.Error switch
            {
                ClassSessionError.SectionNotFound or ClassSessionError.LecturerNotFound
                    or ClassSessionError.SessionNotFound =>
                    StatusCodes.Status404NotFound,
                ClassSessionError.ScheduleConflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(sessionException.Message, sessionException.Errors),
                cancellationToken);
            return true;
        }

        if (exception is EnrollmentException enrollmentException)
        {
            httpContext.Response.StatusCode = enrollmentException.Error switch
            {
                EnrollmentError.SectionNotFound or EnrollmentError.StudentNotFound =>
                    StatusCodes.Status404NotFound,
                EnrollmentError.NotAssigned => StatusCodes.Status403Forbidden,
                EnrollmentError.AlreadyEnrolled or EnrollmentError.SectionClosed or EnrollmentError.SectionFull =>
                    StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(enrollmentException.Message),
                cancellationToken);
            return true;
        }

        if (exception is GradeComponentException componentException)
        {
            httpContext.Response.StatusCode = componentException.Error switch
            {
                GradeComponentError.SectionNotFound => StatusCodes.Status404NotFound,
                GradeComponentError.AlreadyExists or GradeComponentError.WeightExceeded or GradeComponentError.Busy =>
                    StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(componentException.Message), cancellationToken);
            return true;
        }

        if (exception is GradeException gradeException)
        {
            httpContext.Response.StatusCode = gradeException.Error switch
            {
                GradeError.SectionNotFound => StatusCodes.Status404NotFound,
                GradeError.NotAssigned => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Failed(gradeException.Message), cancellationToken);
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
