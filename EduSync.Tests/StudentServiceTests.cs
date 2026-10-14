using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Students;
using EduSync.Application.Services.Students;
using EduSync.Domain.Enums;
using EduSync.Infrastructure.Authentication;
using Xunit;

namespace EduSync.Tests;

public sealed class StudentServiceTests
{
    [Fact]
    public async Task Create_SavesStudentAccountWithoutIssuingRefreshToken()
    {
        var majorId = Guid.NewGuid();
        var accounts = new AuthServiceTests.FakeAuthRepository(majorId);
        var passwords = new IdentityPasswordService();
        var service = new StudentService(new StubStudentRepository(), accounts, passwords);
        var command = new CreateStudentCommand(
            " New.Student@Example.com ", "Password123!", " STU002 ", " Nguyen Van B ",
            majorId, 2026, null, null, null);

        var result = await service.CreateAsync(command);

        Assert.Equal("new.student@example.com", accounts.User?.Email);
        Assert.Equal(UserRole.Student, accounts.User?.Role.Name);
        Assert.Equal(accounts.User?.Id, accounts.Student?.UserId);
        Assert.True(passwords.Verify(accounts.User!, command.Password));
        Assert.Null(accounts.RefreshToken);
        Assert.Equal(accounts.Student?.Id, result.Id);
        Assert.Equal("STU002", result.StudentCode);
        Assert.Equal("Nguyen Van B", result.FullName);
    }

    [Fact]
    public async Task Create_RejectsMissingMajorWithoutSavingAccount()
    {
        var accounts = new AuthServiceTests.FakeAuthRepository(Guid.NewGuid());
        var service = new StudentService(
            new StubStudentRepository(), accounts, new IdentityPasswordService());
        var command = new CreateStudentCommand(
            "student@example.com", "Password123!", "STU002", "Nguyen Van B",
            Guid.NewGuid(), 2026, null, null, null);

        var exception = await Assert.ThrowsAsync<AuthException>(() => service.CreateAsync(command));

        Assert.Equal(AuthError.InvalidMajor, exception.Error);
        Assert.Null(accounts.User);
        Assert.Null(accounts.Student);
    }

    private sealed class StubStudentRepository : IStudentRepository
    {
        public Task<StudentListDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }
}
