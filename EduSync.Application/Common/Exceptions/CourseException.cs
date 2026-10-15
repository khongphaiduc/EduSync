namespace EduSync.Application.Common.Exceptions;

public sealed class CourseException(CourseError error, string message) : Exception(message)
{
    public CourseError Error { get; } = error;
}
