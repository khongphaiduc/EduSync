namespace EduSync.Application.Common.Exceptions;

public sealed class AuthException(AuthError error, string message) : Exception(message)
{
    public AuthError Error { get; } = error;
}
