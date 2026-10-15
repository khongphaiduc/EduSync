namespace EduSync.Application.Common.Exceptions;

public enum AuthError
{
    InvalidCredentials,
    InactiveAccount,
    InvalidRefreshToken,
    EmailAlreadyExists,
    StudentCodeAlreadyExists,
    LecturerCodeAlreadyExists,
    InvalidMajor,
    InvalidRegistration,
    RegistrationConflict
}
