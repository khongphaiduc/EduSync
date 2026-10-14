using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Authentication;

public interface IAuthTokenService
{
    IssuedTokens Issue(UserEntity user);

    string HashRefreshToken(string refreshToken);
}
