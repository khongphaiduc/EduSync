namespace EduSync.Application.Common.Exceptions;

public sealed class EnrollmentException(EnrollmentError error, string message) : Exception(message)
{
    public EnrollmentError Error { get; } = error;
}
