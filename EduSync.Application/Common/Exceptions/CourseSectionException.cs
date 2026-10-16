namespace EduSync.Application.Common.Exceptions;

public sealed class CourseSectionException(CourseSectionError error, string message) : Exception(message)
{
    public CourseSectionError Error { get; } = error;
}
