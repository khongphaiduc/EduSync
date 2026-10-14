using EduSync.Domain.Enums;

namespace EduSync.Application.DTOs.Auth;

public sealed record AuthUserDto(Guid Id, string Email, UserRole Role);
