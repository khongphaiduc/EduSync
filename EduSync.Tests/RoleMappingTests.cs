using EduSync.Domain.Enums;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Xunit;

namespace EduSync.Tests;

public sealed class RoleMappingTests
{
    [Theory]
    [InlineData("Admin", UserRole.Admin)]
    [InlineData("Lecturer", UserRole.Lecturer)]
    [InlineData("Student", UserRole.Student)]
    public void PersistedRoleName_MapsToEnum(string name, UserRole expected)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = name };

        Assert.Equal(expected, role.ToDomainEntity().Name);
    }

    [Fact]
    public void UnknownPersistedRoleName_IsRejected()
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "Unknown" };

        Assert.Throws<InvalidOperationException>(() => role.ToDomainEntity());
    }
}
