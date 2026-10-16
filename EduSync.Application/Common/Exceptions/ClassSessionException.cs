namespace EduSync.Application.Common.Exceptions;

public sealed class ClassSessionException(
    ClassSessionError error,
    string message,
    IReadOnlyDictionary<string, string[]>? errors = null) : Exception(message)
{
    public ClassSessionError Error { get; } = error;

    public IReadOnlyDictionary<string, string[]>? Errors { get; } = errors;
}
