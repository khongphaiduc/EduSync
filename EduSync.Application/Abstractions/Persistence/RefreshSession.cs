using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Persistence;

public sealed record RefreshSession(UserEntity User, RefreshTokenEntity Token);
