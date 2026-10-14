using EduSync.Domain.Enums;

namespace EduSync.Domain.Entities;

public class RoleEntity
{
    public Guid Id { get; set; }

    public UserRole Name { get; set; }
}
