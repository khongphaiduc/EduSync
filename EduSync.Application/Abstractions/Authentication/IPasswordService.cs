using EduSync.Domain.Entities;

namespace EduSync.Application.Abstractions.Authentication;

public interface IPasswordService
{
    string Hash(UserEntity user, string password);

    bool Verify(UserEntity user, string password);
}
