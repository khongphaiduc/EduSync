namespace EduSync.Application.Common.Exceptions;

public sealed class AttendanceException(AttendanceError error, string message) : Exception(message)
{
    public AttendanceError Error { get; } = error;
}
