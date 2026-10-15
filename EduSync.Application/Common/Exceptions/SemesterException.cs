namespace EduSync.Application.Common.Exceptions;

public sealed class SemesterException(SemesterError error, string message) : Exception(message)
{
    public SemesterError Error { get; } = error;
}
