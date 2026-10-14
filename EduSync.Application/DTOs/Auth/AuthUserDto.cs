namespace EduSync.Application.DTOs.Auth;

public sealed record AuthUserDto(Guid Id, string Email, string Role);
