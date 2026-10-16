namespace EduSync.Application.Common.Exceptions;

public sealed class GradeComponentException(GradeComponentError error, string message) : Exception(message)
{
    public GradeComponentError Error { get; } = error;
}
