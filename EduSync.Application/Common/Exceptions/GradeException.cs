namespace EduSync.Application.Common.Exceptions;

public sealed class GradeException(GradeError error, string message) : Exception(message)
{
    public GradeError Error { get; } = error;
}
